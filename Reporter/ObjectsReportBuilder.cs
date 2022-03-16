using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using OPCWrapper;
using OPCWrapper.DataAccess;
using OPCWrapper.HistoricalDataAccess;

namespace Reporter
{
    class ObjectsReportBuilder : ReportBuilderBase
    {
        private readonly OpcDaClient _opcDaClient;
        private readonly OpcHdaClient _opcHdaClient;
        private Logger _logger = LogManager.GetLogger("MainLog");

        public delegate void NotifyHandler(double progressValue);
        public event NotifyHandler NotifyProgress;
        private int _objectsCount = 0;

        private DateTime _startTime;
        private DateTime _endTime;

        public ObjectsReportBuilder(OpcDaClient daClient, OpcHdaClient hdaClient)
        {
            _opcDaClient = daClient;
            _opcHdaClient = hdaClient;
        }

        public async Task<Report> GetReportAsync(ReportConfiguration reportConfiguration, DateTime startTime, DateTime endTime)
        {
            _objectsCount = reportConfiguration.ObjectsConfigurations.Count;
            _startTime = startTime;
            _endTime = endTime;

            //var protHistoryTask = new Task<IEnumerable<ReportObject>>.Run(() => );
            var taskResult = await Task.Run(() => GetHistoryOfProtections(reportConfiguration, startTime, endTime));
            var currentReport = new Report(reportConfiguration.Name, taskResult);
            return currentReport;
        }


        private IEnumerable<ReportObject> GetHistoryOfProtections(ReportConfiguration reportConfiguration, DateTime timeStart, DateTime timeEnd)
        {
            var objsConfigurations = reportConfiguration.ObjectsConfigurations;

            var reportObjects = new List<ReportObject>();
            var tagNames = objsConfigurations.Select(s => s.Tag).ToList();
            int counter = 1;
            foreach (var objConfig in objsConfigurations)
            {
                try
                {
                    var tagName = objConfig.Tag;

                    var history = _opcHdaClient.ReadRaw(timeStart, timeEnd, new List<string>() { tagName });
                    if (objConfig.Type == ObjectType.CommonObj)
                    {
                        var commonObjects = GetCommonProtObjects(history[0]);
                        reportObjects.AddRange(commonObjects);
                    }

                    if (objConfig.Type == ObjectType.SecondLevelTZ1 || objConfig.Type == ObjectType.SecondLevelTZ2)
                    {
                        var secondLevelProtections = GetSecondLevelObjects(history[0]);
                        reportObjects.AddRange(secondLevelProtections);
                    }

                    if (objConfig.Type == ObjectType.TorFlag)
                    {
                        var torObjects = GetTorObjects(history[0]);
                        reportObjects.AddRange(torObjects);
                    }
                    NotifyProgress?.Invoke((double)counter / _objectsCount * 100);
                    counter++;
                }
                catch (Exception ex)
                {
                    _logger.Error($"Cannot process: {objConfig.Tag} => {ex}");
                }
            }
            return reportObjects;
        }

        private List<ReportObject> GetCommonProtObjects(OpcHdaResultsCollection historyResults)
        {
            var opcTagsReadBuffer = new List<string>();
            var buffer = new List<ReportObject>();
            if (historyResults.IsSuccess)
            {
                foreach (OpcHdaResultItem result in historyResults)
                {
                    if (result.Value == null)
                        continue;

                    bool parseResult = bool.TryParse(result.Value.ToString(), out bool currentValue);
                    if (parseResult)
                    {
                        opcTagsReadBuffer.Add(historyResults.ItemName + ".Description");
                        var prevProt = buffer.LastOrDefault();
                        // Имеется признак установки
                        if (currentValue)
                        {
                            // если предыдущая защита в буфере отсутствует
                            // если уже есть, при этом она гуд (имеется признак снятия и установки), то добавляем новую
                            if (prevProt == null || prevProt.TimeUnset != DateTime.MinValue)
                            {
                                buffer.Add(new CommonObject
                                {
                                    Tag = historyResults.ItemName,
                                    TimeSet = result.Timestamp < _startTime ? new DateTime(_startTime.Year, _startTime.Month, _startTime.Day, 0, 0, 0) : result.Timestamp
                                });
                            }
                        }
                        // Имеется признак снятия
                        else
                        {
                            if (prevProt != null && prevProt.TimeUnset == DateTime.MinValue)
                                prevProt.TimeUnset = result.Timestamp;
                        }
                    }
                }
            }

            // empty prot object for good list
            if (!buffer.Any())
            {
                buffer.Add(GetEmptyCommonObject(historyResults.ItemName));
                opcTagsReadBuffer.Add(historyResults.ItemName + ".Description");
            }

            // Читаем из OPC DA и проставляем имен
            ProcessObjectsWithBadUnsetTime(buffer);
            SetNamesForBufferedObjects(opcTagsReadBuffer, buffer);

            return buffer;
        }


        private List<ReportObject> GetSecondLevelObjects(OpcHdaResultsCollection historyResults)
        {
            var opcTagsReadBuffer = new List<string>();
            var buffer = new List<ReportObject>();
            var itemName = historyResults.ItemName;
            if (historyResults.IsSuccess)
            {
                var currentFlagNumber = int.Parse(string.Join("", itemName.Split('.').LastOrDefault().Where(c => char.IsDigit(c))));
                foreach (var result in historyResults.FilterResults(FilterType.ValueNotNull))
                {
                    bool parseResult = int.TryParse(result.Value.ToString(), out int currentFlagValue);
                    var bitsValues = CheckBits(currentFlagValue);
                    foreach (var bit in bitsValues)
                    {
                        var protectionNumber = currentFlagNumber * 32 + bit.Key;
                        opcTagsReadBuffer.Add(string.Join(".", itemName.Split('.').Take(4)) + $".LCK_SECOND_LEVEL.state.N{protectionNumber:0000}.Description");
                        var prevProt = buffer.LastOrDefault(p => (p as SecondLevelProtection).Number == protectionNumber);
                        // Значение бита - True, значит маска была установлена 
                        if (bit.Value)
                        {
                            // Проверяем предыдущую такую же защиту в буфере масок
                            if (prevProt == null || prevProt.TimeUnset != DateTime.MinValue)
                            {
                                buffer.Add(new SecondLevelProtection()
                                {
                                    FlagNumber = currentFlagNumber,
                                    BitNumber = bit.Key,
                                    TimeSet = result.Timestamp < _startTime ? new DateTime(_startTime.Year, _startTime.Month, _startTime.Day, 0, 0, 0) : result.Timestamp,
                                    Tag = string.Join(".", itemName.Split('.').Take(4)) + $".LCK_SECOND_LEVEL.state.N{protectionNumber:0000}",
                                    Type = itemName.ToLower().Contains("tz1") ? ObjectType.SecondLevelTZ1 : ObjectType.SecondLevelTZ2
                                });
                            }
                        }
                        // Значение бита - False (маска была снята)
                        else
                        {
                            if (prevProt != null && prevProt.TimeUnset == DateTime.MinValue)
                                prevProt.TimeUnset = result.Timestamp;
                        }
                    }
                }

                ProcessObjectsWithBadUnsetTime(buffer);
                SetNamesForBufferedObjects(opcTagsReadBuffer, buffer);
            }
            return buffer;
        }
        private CommonObject GetEmptyCommonObject(string tag)
        {
            return new CommonObject()
            {
                Tag = tag,
                TimeSet = DateTime.MinValue,
                TimeUnset = DateTime.MinValue
            };
        }
        private List<TorObject> GetTorObjects(OpcHdaResultsCollection historyResults)
        {
            var opcTagsReadBuffer = new List<string>();
            var itemName = historyResults.ItemName;
            var buffer = GetEmptyTorObjects(itemName, out List<string> emptyObjsOpcTagsBuffer);
            opcTagsReadBuffer.AddRange(emptyObjsOpcTagsBuffer);
            if (historyResults.IsSuccess)
            {
                var currentFlagNumber = int.Parse(string.Join("", itemName.Split('.').LastOrDefault().Where(c => char.IsDigit(c))));
                foreach (var result in historyResults.FilterResults(FilterType.ValueNotNull))
                {
                    bool parseResult = int.TryParse(result.Value.ToString(), out int currentFlagValue);
                    var bitsValues = CheckBits(currentFlagValue);
                    foreach (var bit in bitsValues)
                    {
                        var number = currentFlagNumber * 32 + bit.Key;
                        string tagName = itemName + $".b{bit.Key:00}";
                        opcTagsReadBuffer.Add(tagName + ".Description");
                        var prevObject = buffer.LastOrDefault(p => p.Number == number && p.Tag == tagName);
                        if (bit.Value) // Бит установлен
                        {
                            if (prevObject != null)
                            {
                                if (prevObject.TimeSet == DateTime.MinValue)
                                    prevObject.TimeSet = result.Timestamp < _startTime ? new DateTime(_startTime.Year, _startTime.Month, _startTime.Day, 0, 0, 0) : result.Timestamp;
                                if (prevObject.TimeSet != DateTime.MinValue && prevObject.TimeUnset != DateTime.MinValue)
                                {
                                    buffer.Add(new TorObject()
                                    {
                                        Tag = tagName,
                                        FlagNumber = currentFlagNumber,
                                        BitNumber = bit.Key,
                                        TimeSet = result.Timestamp < _startTime ? new DateTime(_startTime.Year, _startTime.Month, _startTime.Day, 0, 0, 0) : result.Timestamp
                                    });
                                }
                            }
                        }
                        else // Бит снят
                        {
                            if (prevObject != null)
                            {
                                if (prevObject.TimeSet != DateTime.MinValue && prevObject.TimeUnset == DateTime.MinValue)
                                    prevObject.TimeUnset = result.Timestamp;
                            }
                        }
                    }
                }

                ProcessObjectsWithBadUnsetTime(buffer);
                SetNamesForBufferedObjects(opcTagsReadBuffer, buffer);
            }
            return buffer;
        }

        private List<TorObject> GetEmptyTorObjects(string itemName, out List<string> opcTagsBuff)
        {
            var opcTagsReadBuffer = new List<string>();
            var emptyObjects = new List<TorObject>();
            var currentFlagNumber = int.Parse(string.Join("", itemName.Split('.').LastOrDefault().Where(c => char.IsDigit(c))));
            // 32 пустых бита
            for (int i = 0; i < 32; i++)
            {
                int number = currentFlagNumber * 32 + i;
                string tagName = itemName + $".b{i:00}";
                opcTagsReadBuffer.Add(tagName + ".Description");

                var currentTorObject = new TorObject()
                {
                    Tag = tagName,
                    FlagNumber = currentFlagNumber,
                    BitNumber = i
                };

                emptyObjects.Add(currentTorObject);
            }
            opcTagsBuff = opcTagsReadBuffer;

            return emptyObjects;
        }


        private static Dictionary<int, bool> CheckBits(int flgValue)
        {
            var bits = new Dictionary<int, bool>();
            // Конвертим в строку, слева добавляем нулевые биты до 32 битов
            var flgBinary = Convert.ToString(flgValue, 2).PadLeft(32, '0');
            // Читаем с конца (от младщего к старшему биту)
            int bitsCount = 0;
            for (int i = flgBinary.Length - 1; i >= 0; i--)
            {
                bits.Add(bitsCount, flgBinary[i] == '1');
                bitsCount++;
            }
            return bits;
        }

        private void SetNamesForBufferedObjects(List<string> opcTagsReadBuffer, IEnumerable<ReportObject> buffer)
        {
            var readResults = _opcDaClient.ReadData(opcTagsReadBuffer.Distinct());
            foreach (var readResult in readResults)
            {
                if (readResult.Value != null)
                {
                    var name = readResult.Value.ToString();
                    buffer.Where(p => p.Tag.ToLower() == readResult.ItemName.ToLower().Replace(".description", "")).ToList().ForEach(a => a.Name = name);
                }
            }
        }

        private void ProcessObjectsWithBadUnsetTime(IEnumerable<ReportObject> buffer)
        {
            // После процессинга всего, проверяем объекты у которых есть время установки и нет времени снятия - значит эти объекты до сих пор имеют флаг установки
            foreach (var obj in buffer)
            {
                if (obj.TimeSet != DateTime.MinValue && obj.TimeUnset == DateTime.MinValue)
                {
                    // проверяем - может мы попали на переломный момент когда есть время установки из нового месяца (не из расчетного)
                    var possibleTimeUnset = new DateTime(_endTime.Year, _endTime.Month, _endTime.Day, 23, 59, 59);
                    while (possibleTimeUnset < obj.TimeSet)
                    {
                        possibleTimeUnset = possibleTimeUnset.AddDays(1);
                    }
                    obj.TimeUnset = possibleTimeUnset;
                }
            }
        }
    }
}
