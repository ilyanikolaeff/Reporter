using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace Reporter
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private Logger _logger = LogManager.GetLogger("MainLog");
        protected override void OnStartup(StartupEventArgs e)
        {
            AppDomain.CurrentDomain.AssemblyLoad += OnAssemblyLoad;
            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
            base.OnStartup(e);
        }

        private Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            string asmLocation = Assembly.GetExecutingAssembly().Location;
            string asmName = args.Name.Substring(0, args.Name.IndexOf(','));
            string fileName = Path.Combine(asmLocation, asmName);

            if (File.Exists(fileName))
            {
                _logger.Debug($"Assembly exists => {fileName}");
                return Assembly.LoadFrom(fileName);
            }
            else
            {
                _logger.Debug($"Assembly does not exist => {fileName}");
                return null;
            }
        }

        private void OnAssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            _logger.Debug($"Assembly loaded: {args.LoadedAssembly.FullName}, {args.LoadedAssembly.Location}");
        }
    }
}
