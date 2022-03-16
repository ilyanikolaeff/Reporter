using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reporter
{
    class Report
    {
        public string Name { get; set; }
        public List<ObjectsGroup> GroupedProtections { get; set; }
        public IEnumerable<ReportObject> ReportObjects { get; set; }

        public Report(string name, IEnumerable<ReportObject> reportObjects)
        {
            ReportObjects = reportObjects;
            Name = name;

            GroupObjects();
        }

        private void GroupObjects()
        {
            var grouper = new ObjectGrouper();
            GroupedProtections = grouper.GroupsObjects(ReportObjects, Name);
        }
    }
}
