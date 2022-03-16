using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Reporter
{
    class ConfigurationProvider
    {
        private static readonly string _fileName = AppDomain.CurrentDomain.BaseDirectory + "Settings.xml";
        public List<ReportConfiguration> GetReportsConfiguration()
        {
            var reports = new List<ReportConfiguration>();

            var xRoot = XDocument.Load(_fileName).Root;
            var xReports = xRoot.Element("Reports").Elements("Report");

            foreach (var xReport in xReports)
            {
                var currentReport = new ReportConfiguration
                {
                    Name = xReport.Attribute("Name").Value,
                    IsNeed = Convert.ToBoolean(xReport.Attribute("IsNeed").Value)
                };

                var xProtections = xReport.Element("Objects").Elements("Object");
                foreach (var xProt in xProtections)
                {
                    var currentProt = new ObjectConfiguration
                    {
                        Tag = xProt.Attribute("Tag").Value,
                        Type = (ObjectType)Enum.Parse(typeof(ObjectType), xProt.Attribute("Type").Value)
                    };
                    currentReport.ObjectsConfigurations.Add(currentProt);
                }

                reports.Add(currentReport);
            }

            return reports;
        }
        public List<GroupsConfiguration> GetGroupsConfigurations()
        {
            var groupsConfsList = new List<GroupsConfiguration>();

            //var xRoot = XDocument.Load(_fileName).Root;
            //var xGroups = xRoot.Element("Groups").Elements("Group");

            //foreach (var xGroup in xGroups)
            //{
            //    var currentGroup = new Group
            //    {
            //        Type = (ObjectType)Enum.Parse(typeof(ObjectType), xGroup.Attribute("Type").Value)
            //    };

            //    var ranges = new List<Range>();
            //    var xRanges = xGroup.Elements("Range");
            //    foreach (var xRange in xRanges)
            //    {
            //        ranges.Add(new Range()
            //        {
            //            Name = xRange.Attribute("Name").Value,
            //            Value = xRange.Attribute("Value").Value
            //        });
            //    }
            //    currentGroup.Ranges = ranges;
            //    groupsConfsList.Add(currentGroup);
            //}

            return groupsConfsList;
        }
    }
}
