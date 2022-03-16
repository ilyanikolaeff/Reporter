using OPCWrapper.HistoricalDataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reporter
{
    class AlgorithmsReportBuilder : ReportBuilderBase
    {
        public IEnumerable<AlgorithmReportObject> GetAlgorithms(DateTime timeStart, DateTime timeEnd, OpcHdaClient opcHdaClient, List<string> tags, string tzName)
        {
            var algorithms = new List<AlgorithmReportObject>();

            // take history (0) - codes, (1) - statuse, (2) - plcPassive
            var historyResults = opcHdaClient.ReadRaw(timeStart, timeEnd, tags);

            var codes = historyResults[0];
            var statuses = historyResults[1];
            var plcStatuses = historyResults[2];

            var filteredStatuses = statuses.FilterResults(FilterType.ValueNotNull);
            for (int i = 0; i < filteredStatuses.Count - 1; i++)
            {
                var currentItem = filteredStatuses[i];
                var nextItem = filteredStatuses[i + 1];

                bool parseResult;
                parseResult = int.TryParse(currentItem.Value.ToString(), out int currentValue);
                parseResult = int.TryParse(nextItem.Value.ToString(), out int nextValue);

                nextValue = nextValue == 2 ? 5 : nextValue; // проверка на два старта без окончания

                if (currentValue == 2)
                {
                    // check plc passive
                    var plcStatus = plcStatuses.GetResult(currentItem.Timestamp, nextItem.Timestamp, FindType.First, IntervalChangeType.Extension, FilterType.ValueNotNull);
                    bool? isPassive = null;
                    if (plcStatus != null)
                    {
                        var plcStatusParseResult = int.TryParse(plcStatus.Value.ToString(), out int plcStatusValue);
                        if (!plcStatusParseResult)
                            isPassive = null;
                        else
                            isPassive = plcStatusValue == 1;
                    }

                    algorithms.Add(new AlgorithmReportObject()
                    {
                        StartTime = currentItem.Timestamp,
                        EndTime = nextItem.Timestamp,
                        IsPlcPassive = isPassive,
                        Name = $"Противоаварийная остановка {tzName}"
                    });
                }
            }
            return algorithms;
        }
    }
}
