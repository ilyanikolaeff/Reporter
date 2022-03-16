using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


namespace Reporter
{
    class ExportProvider
    {
        public static Task ExportReport(Report report)
        {
            var tasks = new List<Task>
            {
                Task.Run(() => ExportReportMain(report)),
                ExportReportObjects(report.ReportObjects, report.Name)
            };

            return Task.WhenAll(tasks);
        }

        public static void ExportAlgorithms(IEnumerable<AlgorithmReportObject> algorithms)
        {
            if (algorithms.Count() > 0)
            {
                using (var streamWriter = new StreamWriter($"Alarm_Algrorithms.txt", false))
                {
                    foreach (var algorithm in algorithms)
                    {
                        streamWriter.WriteLine($"{algorithm.Name}\t{algorithm.StartTime}\t{algorithm.EndTime}\t{algorithm.Duration}" +
                            $"\t{algorithm.DurationInSeconds}\t{algorithm.DurationInDays}\t{algorithm.IsPlcPassive}");
                    }
                }
            }
        }

        private static void ExportReportMain(Report report)
        {
            using (var streamWriter = new StreamWriter($"{report.Name}.txt", false))
            {
                streamWriter.WriteLine(report.Name);
                foreach (var group in report.GroupedProtections)
                {
                    streamWriter.WriteLine($"{group.Name}\t{group.Tag}\t{group.ActivationsCount}\t{group.Duration}\t{group.DurationInSeconds}\t{group.DurationInDays}");
                }
            }
        }


        private static void ExportTorObjects(IEnumerable<ReportObject> torObjects, string fileName)
        {
            if (torObjects.Count() > 0)
            {
                using (var streamWriter = new StreamWriter(fileName + "_TorObjsSeparately.txt", false))
                {
                    streamWriter.WriteLine($"Tag\tFlgNumber\tBitNumber\tName\tTimeSet\tTimeUnset\tDuration\tDuration(days)\tDuration(secs)");
                    var orderedObjs = torObjects.OrderBy(ks => ks.Tag);
                    foreach (TorObject torObject in orderedObjs)
                    {
                        streamWriter.WriteLine($"{torObject.Tag}\t{torObject.FlagNumber}\t{torObject.BitNumber}" +
                            $"\t{torObject.Name}\t{torObject.TimeSet}\t{torObject.TimeUnset}" +
                            $"\t{torObject.Duration}\t{torObject.Duration.TotalDays}\t{torObject.Duration.TotalSeconds}");
                    }
                }
            }
        }

        private static void ExportSecondLevelProtections(IEnumerable<ReportObject> secondLevelObjects, string fileName)
        {
            if (secondLevelObjects.Count() > 0)
            {
                using (var streamWriter = new StreamWriter(fileName + "_SecLvlProtsSeparetely.txt", false))
                {
                    streamWriter.WriteLine($"Tag\tFlgNumber\tBitNumber\tName\tTimeSet\tTimeUnset\tDuration\tDuration(days)\tDuration(secs)");
                    //var orderedObjs = secondLevelObjects.OrderBy(ks => ks.Tag);
                    var orderedObjs = secondLevelObjects;
                    foreach (SecondLevelProtection secLevelObj in orderedObjs)
                    {
                        streamWriter.WriteLine($"{secLevelObj.Tag}\t{secLevelObj.FlagNumber}\t{secLevelObj.BitNumber}" +
                            $"\t{secLevelObj.Name}\t{secLevelObj.TimeSet}\t{secLevelObj.TimeUnset}" +
                            $"\t{secLevelObj.Duration}\t{secLevelObj.Duration.TotalDays}\t{secLevelObj.Duration.TotalSeconds}");
                    }
                }
            }
        }

        private static void ExportCommonObjects(IEnumerable<ReportObject> commonProtOjects, string fileName)
        {
            if (commonProtOjects.Count() > 0)
            {
                using (var streamWriter = new StreamWriter(fileName + "_CommObjsSeparately.txt", false))
                {
                    streamWriter.WriteLine($"Tag\tName\tTimeSet\tTimeUnset\tDuration\tDuration(days)\tDuration(secs)");
                    //var orderedObjs = commonProtOjects.OrderBy(ks => ks.Tag);
                    var orderedObjs = commonProtOjects;
                    foreach (CommonObject torObject in orderedObjs)
                    {
                        streamWriter.WriteLine($"{torObject.Tag}" +
                            $"\t{torObject.Name}\t{torObject.TimeSet}\t{torObject.TimeUnset}" +
                            $"\t{torObject.Duration}\t{torObject.Duration.TotalDays}\t{torObject.Duration.TotalSeconds}");
                    }
                }
            }
        }

        private static Task ExportReportObjects(IEnumerable<ReportObject> reportObjects, string reportName)
        {
            var commObjs = reportObjects.Where(p => p is CommonObject);
            var slObjects = reportObjects.Where(p => p is SecondLevelProtection);
            var torObjs = reportObjects.Where(p => p is TorObject);
            var tasks = new List<Task>
            {
                Task.Run(() => ExportCommonObjects(commObjs, reportName)),
                Task.Run(() => ExportSecondLevelProtections(slObjects, reportName)),
                Task.Run(() => ExportTorObjects(torObjs, reportName))
            };

            return Task.WhenAll(tasks);
        }
    }
}
