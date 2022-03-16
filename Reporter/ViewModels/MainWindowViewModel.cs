using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using NLog;
using OPCWrapper.DataAccess;
using OPCWrapper;
using OPCWrapper.HistoricalDataAccess;
using System.Windows;

namespace Reporter
{
    class MainWindowViewModel : ViewModelBase
    {
        public DateTime SelectedStartTime
        {
            get => Settings.StartTime;
            set => Settings.StartTime = value;
        }
        public DateTime SelectedEndTime
        {
            get => Settings.EndTime;
            set => Settings.EndTime = value;
        }
        public string DA_IpAddress
        {
            get => Settings.OpcDaIpAddress;
            set { Settings.OpcDaIpAddress = value; }
        }
        public string DA_ServerName
        {
            get => Settings.OpcDaServerName;
            set { Settings.OpcDaServerName = value; }
        }
        public string HDA_IpAddress
        {
            get => Settings.OpcHdaIpAddress;
            set { Settings.OpcHdaIpAddress = value; }
        }
        public string HDA_ServerName
        {
            get => Settings.OpcHdaServerName;
            set { Settings.OpcHdaServerName = value; }
        }

        private Logger _logger = LogManager.GetCurrentClassLogger();

        public MainWindowViewModel()
        {
            SelectedStartTime = new DateTime(DateTime.Now.AddMonths(-1).Year, DateTime.Now.AddMonths(-1).Month, 1, 0, 0, 0);
            SelectedEndTime = new DateTime(DateTime.Now.AddMonths(-1).Year, DateTime.Now.AddMonths(-1).Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.AddMonths(-1).Month), 23, 59, 59);

            ShowAlgorithmsReportCommand = new DelegateCommand(ShowAlgorithmsReport);
            WorkWithReportsCommand = new DelegateCommand(ShowReports);
        }


        public ICommand ShowAlgorithmsReportCommand { get; private set; }

        private void ShowAlgorithmsReport()
        {
            try
            {
                var vm = new AlgorithmReportViewModel(SelectedStartTime, SelectedEndTime);
                var view = new AlgorithmReport
                {
                    DataContext = vm
                };
                view.Show();
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        public ICommand WorkWithReportsCommand { get; private set; }

        private void ShowReports()
        {
            var msgBoxString = String.Empty;
            msgBoxString += $"Дата начала запроса: {SelectedStartTime}\n";
            msgBoxString += $"Дата окончания запроса: {SelectedEndTime}\n";
            msgBoxString += $"HDA conn: {HDA_IpAddress}\\{HDA_ServerName}\n";
            msgBoxString += $"DA conn: {DA_IpAddress}\\{DA_ServerName}\n";
            msgBoxString += $"Будет выполнено подключение к указанным серверам, продолжить? (может занять длительное время)";


            if (MessageBox.Show(msgBoxString, "ИНФО", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
            {
                var view = new ReportBuilderView();
                var viewModel = new ReportBuilderViewModel();
                view.DataContext = viewModel;
                view.ShowDialog();
            }
        }
    }
}
