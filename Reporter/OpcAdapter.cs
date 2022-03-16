using OPCWrapper;
using OPCWrapper.DataAccess;
using OPCWrapper.HistoricalDataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace Reporter
{
    class OpcAdapter
    {
        private static OpcDaClient _daClientInstance;
        public static OpcDaClient GetOpcDaClient(out bool isConnected)
        {
            if (_daClientInstance == null
                || _daClientInstance.ConnectionSettings.IPAddress != Settings.OpcDaIpAddress || _daClientInstance.ConnectionSettings.ServerName != Settings.OpcDaServerName)
                _daClientInstance = ConnectOpcDa();


            isConnected = _daClientInstance.IsConnected;
            return _daClientInstance;
        }

        private static OpcHdaClient _hdaClientInstance;
        public static OpcHdaClient GetOpcHdaClient(out bool isConnected)
        {
            if (_hdaClientInstance == null 
                || _hdaClientInstance.ConnectionSettings.IPAddress != Settings.OpcHdaIpAddress || _hdaClientInstance.ConnectionSettings.ServerName != Settings.OpcHdaServerName)   
                _hdaClientInstance = ConnectOpcHda();

            isConnected = _hdaClientInstance.IsConnected;
            return _hdaClientInstance;
        }



        private static OpcDaClient ConnectOpcDa()
        {
            var opcDaClient = new OpcDaClient(new ConnectionSettings(Settings.OpcDaIpAddress, Settings.OpcDaServerName));
            opcDaClient.Connect();
            return opcDaClient;
        }

        private static OpcHdaClient ConnectOpcHda()
        {
            var opcHdaClient = new OpcHdaClient(new ConnectionSettings(Settings.OpcHdaIpAddress, Settings.OpcHdaServerName));
            opcHdaClient.Connect();
            return opcHdaClient;
        }
    }
}
