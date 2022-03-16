using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reporter
{
    class AlgorithmReportObject : ReportObjectBase
    {
        public string Name { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Duration { get => EndTime - StartTime; }
        public double DurationInSeconds { get => Duration.TotalSeconds; }
        public double DurationInDays { get => Duration.TotalDays; }
        public bool? IsPlcPassive { get; set; }
    }
}
