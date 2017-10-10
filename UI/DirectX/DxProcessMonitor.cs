using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using System.Windows.Threading;

namespace UI.DirectX
{
    public class DxProcessMonitor
    {
        private static readonly string[] DxModuleNames = { "d3d9.dll", "d3d11.dll" };

        public ObservableCollection<Process> Processes { get; }

        private readonly Timer _timer;
        private readonly Dispatcher _dispatcher;

        public DxProcessMonitor(Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
            Processes = new ObservableCollection<Process>();
            UpdateProcessList();

            _timer = new Timer(1000);
            _timer.Elapsed += (sender, args) => UpdateProcessList();
        }

        public void Start()
        {
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }

        private void UpdateProcessList()
        {
            foreach (var process in Process.GetProcesses())
            {
                try
                {
                    // We only want applications with windows
                    if (string.IsNullOrEmpty(process.MainWindowTitle))
                        continue;

                    if (process.Modules
                        .Cast<ProcessModule>()
                        .Any(module => DxModuleNames.Any(x => module.ModuleName.Equals(x, StringComparison.InvariantCultureIgnoreCase))))
                    {
                        ProcessFound(process, EventArgs.Empty);
                    }
                }
                catch
                {
                    // No modules
                }
            }
        }
        
        private void ProcessFound(object sender, EventArgs e)
        {
            var process = (Process)sender;

            if (Processes.Any(x => x.ProcessName.Equals(process.ProcessName)))
                return;

            _dispatcher.Invoke(
                () => Processes.Add(process));

            process.Exited += ProcessExited;
            process.EnableRaisingEvents = true;
        }

        private void ProcessExited(object sender, EventArgs e)
        {
            var process = (Process)sender;
            _dispatcher.Invoke(
                () => Processes.Remove(process));
        }
    }
}
