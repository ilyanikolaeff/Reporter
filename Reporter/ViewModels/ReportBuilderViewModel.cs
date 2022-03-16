using DevExpress.Mvvm;
using OPCWrapper.DataAccess;
using OPCWrapper.HistoricalDataAccess;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace Reporter
{
    class ReportBuilderViewModel : ViewModelBase
    {
        public double ProgressValue
        {
            get => GetValue<double>();
            set => SetValue(value);
        }

        public string State
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public bool DaConnectionStatus
        {
            get
            {
                if (_opcDaClient == null)
                    return false;
                else
                    return _opcDaClient.IsConnected;
            }
            set => SetValue(value);
        }

        public bool HdaConnectionStatus
        {
            get
            {
                if (_opcHdaClient == null)
                    return false;
                else
                    return _opcHdaClient.IsConnected;
            }
            set => SetValue(value);
        }

        public ObservableCollection<ReportConfiguration> ReportsConfigurations { get; set; }


        public ICommand GetReportConfigurationsCommand { get; private set; }
        public ICommand MakeReportsCommand { get; private set; }

        public ReportBuilderViewModel()
        {
            Connect();
            RegisterCommands();
            LoadReportsConfigurations();

            SetProgress("Готов к работе. Выберите отчеты и нажмите кнопку сформировать");
        }

        private OpcDaClient _opcDaClient;
        private OpcHdaClient _opcHdaClient;

        private void RegisterCommands()
        {
            GetReportConfigurationsCommand = new DelegateCommand(GetReportConfigurations);
            MakeReportsCommand = new AsyncCommand(
                () => MakeReportsAsync(),
                () => ReportsConfigurations != null && ReportsConfigurations.Any(p => p.IsNeed) && DaConnectionStatus && HdaConnectionStatus);
        }

        private void Connect()
        {
            _opcDaClient = OpcAdapter.GetOpcDaClient(out bool daIsConnected);
            _opcHdaClient = OpcAdapter.GetOpcHdaClient(out bool hdaIsConnected);
            if (!daIsConnected || !hdaIsConnected)
                return;
        }

        private void LoadReportsConfigurations()
        {
            try
            {
                ReportsConfigurations = new ObservableCollection<ReportConfiguration>();
                var configProvider = new ConfigurationProvider();
                var reportsConfigs = configProvider.GetReportsConfiguration();
                foreach (var item in reportsConfigs)
                    ReportsConfigurations.Add(item);
                RaisePropertiesChanged();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error => {ex}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GetReportConfigurations()
        {
            LoadReportsConfigurations();
        }

        private async Task MakeReportsAsync()
        {
            try
            {
                int reportsCount = ReportsConfigurations.Where(p => p.IsNeed).Count();
                if (!(reportsCount > 0))
                    return;

                GC.Collect();
                GC.WaitForPendingFinalizers();

                ProgressValue = 0;
                State = "";


                var reportProvider = new ObjectsReportBuilder(_opcDaClient, _opcHdaClient);
                reportProvider.NotifyProgress += ReportProvider_NotifyProgress;
                var tasksList = new List<Task<Report>>();


                int counter = 1;

                foreach (var reportConfiguration in ReportsConfigurations)
                {
                    if (reportConfiguration.IsNeed)
                    {
                        SetProgress($"[{counter}/{reportsCount}] Обработка отчета: [{reportConfiguration.Name}]");
                        var report = await reportProvider.GetReportAsync(reportConfiguration, Settings.StartTime, Settings.EndTime);
                        await ExportProvider.ExportReport(report);
                        counter++;
                    }
                }

                SetProgress($"Все отчеты сформированы. Готов к новым задачам", 100);
                MessageBox.Show($"Выбранные отчеты:\n{string.Join(Environment.NewLine, ReportsConfigurations.Where(p => p.IsNeed).Select(s => s.Name))}{Environment.NewLine}успешно сформированы!",
                    "Информация об успехе", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error => {ex}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ReportProvider_NotifyProgress(double progressValue)
        {
            ProgressValue = Math.Round(progressValue, 2);
        }

        private void SetProgress(string progressText, double progressValue = 0)
        {
            State = progressText;
            ProgressValue = progressValue;
            RaisePropertiesChanged();
        }
    }
}
