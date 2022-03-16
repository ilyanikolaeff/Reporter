using DevExpress.Mvvm;
using NLog;
using OPCWrapper;
using OPCWrapper.HistoricalDataAccess;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reporter
{
    class AlgorithmReportViewModel : ViewModelBase
    {
        public ObservableCollection<AlgorithmReportObject> Algorithms { get; set; } 
        public AlgorithmReportViewModel(DateTime start, DateTime end)
        {
            var opcHdaClient = OpcAdapter.GetOpcHdaClient(out bool isConnected);
            if (!isConnected)
            {
                return;
            }

            var algReportProvider = new AlgorithmsReportBuilder();

            Algorithms = new ObservableCollection<AlgorithmReportObject>();

            var tz1Tags = new List<string>()
            {
                "ak.vsmn.ucs.tz1.task_alm_close",
                "ak.vsmn.ucs.tz1.task_alm_close.status",
                "ak.vsmn.ucs.tz1.plc.status"
            };
            var tz1Algs = algReportProvider.GetAlgorithms(start, end, opcHdaClient, tz1Tags, "ТУ-1");

            var tz2Tags = new List<string>()
            {
                "ak.vsmn.ucs.tz2.task_alm_close",
                "ak.vsmn.ucs.tz2.task_alm_close.status",
                "ak.vsmn.ucs.tz2.plc.status"
            };
            var tz2Algs = algReportProvider.GetAlgorithms(start, end, opcHdaClient, tz2Tags, "ТУ-2");

            foreach (var alg in tz1Algs)
                Algorithms.Add(alg);
            foreach (var alg in tz2Algs)
                Algorithms.Add(alg);

            if (Algorithms.Any())
                ExportProvider.ExportAlgorithms(Algorithms);
        }

    }
}
