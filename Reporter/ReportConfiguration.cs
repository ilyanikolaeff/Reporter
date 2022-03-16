using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Reporter
{
    class ReportConfiguration
    {
        public string Name { get; set; }
        public bool IsNeed { get; set; }
        public List<ObjectConfiguration> ObjectsConfigurations { get; set; }
        public ReportConfiguration()
        {
            ObjectsConfigurations = new List<ObjectConfiguration>();
        }
    }
    class ObjectConfiguration
    {
        public string Tag { get; set; }
        public ObjectType Type { get; set; }
    }
    enum ObjectType
    {
        CommonObj,
        SecondLevelTZ1,
        SecondLevelTZ2,
        TorFlag
    }
}
