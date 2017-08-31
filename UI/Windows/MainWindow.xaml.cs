using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Ipc;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using EasyHook;
using Hook;
using Hook.D3D11;
using Color = System.Windows.Media.Color;

namespace UI.Windows
{
    public partial class MainWindow
    {
        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr windowHandle, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr windowHandle);

        private string _saveFilePath;
        private SaveModelWindow _saveModelWindow;
        private IpcServerChannel _server;
        private ServerInterface _serverInterface;
        private Process _selectedProcess;
        private IntPtr _windowHandle;

        public MainWindow()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            _saveFilePath = Path.Combine(Directory.GetCurrentDirectory(), "savedModels.txt");

            _windowHandle = Process.GetCurrentProcess().MainWindowHandle;

            _saveModelWindow = new SaveModelWindow();
            _saveModelWindow.OnSaveModel += OnSaveModel;

            _serverInterface = new ServerInterface();
            _serverInterface.MessageRecieved += MessageRecieved;
            _serverInterface.HookStarted += HookStarted;
            _serverInterface.SaveModelRequestRecieved += SaveModelRequestRecieved;

            UpdateProcesses();

            WriteToLog("Controls:\n" +
                       "End = Toggle Model Logger\n" +
                       "\tNumpad 1 = Decrease Byte Width\n" +
                       "\tNumpad 3 = Increase Byte Width\n" +
                       "\tNumpad 2 = Previous Model\n" +
                       "\tNumpad 5 = Next Model\n" +
                       "\tInsert = Save Current Model\n" +
                       "Numpad 0 = Render Only Saved Models\n",
                       null);
        }

        private void FocusWindow()
        {
            ShowWindow(_selectedProcess.MainWindowHandle, 11);
            ShowWindow(_windowHandle, 1);
            SetForegroundWindow(_windowHandle);
        }

        private void SaveModelRequestRecieved(ModelParameters model)
        {
            if (SaveFile.Load(_saveFilePath, true).Contains(model))
                return;

            Dispatcher.Invoke(() =>
            {
                WriteToLog($"Saving model...\n{model}\n", null);
                _saveModelWindow.Show(model);
                FocusWindow();
            });
        }

        private void Inject()
        {
            string channelName = null;

            _server = RemoteHooking.IpcCreateServer(ref channelName, WellKnownObjectMode.Singleton, _serverInterface);

            var injectionLibrary = Path.Combine(Directory.GetCurrentDirectory(), "Hook.dll");

            try
            {
                WriteToLog("Attempting to inject DLL into process...", null);

                RemoteHooking.Inject(
                    _selectedProcess.Id,
                    injectionLibrary,
                    injectionLibrary,
                    channelName
                    );

                Thread.Sleep(1000);
            }
            catch (Exception e)
            {
                WriteToLog($"There was an error while injecting into target: {e}", new byte[] {255, 0, 0});
            }
        }

        private void UpdateProcesses()
        {
            foreach (var process in Process.GetProcesses())
            {
                try
                {
                    if (!string.IsNullOrEmpty(process.MainWindowTitle))
                        ProcessList.Items.Add(process);
                }
                catch
                {
                    // No modules
                }
            }
        }

        private void HookStarted()
        {
            var savedModels = SaveFile.Load(_saveFilePath, true);

            Dispatcher.Invoke(() =>
            {
                foreach (var model in savedModels.OrderBy(x => x.Name))
                    SavedModelsList.Items.Add(model);
            });

            _serverInterface.ReloadModels(savedModels.Where(x => x.Enabled).ToList());
        }

        private void MessageRecieved(string message, byte[] color)
        {
            Dispatcher.Invoke(() =>
            {
                WriteToLog(message, color);
            });
        }

        private void OnSaveModel(ModelParameters model)
        {
            SaveFile.Save(_saveFilePath, model);
            _serverInterface.ReloadModels(SaveFile.Load(_saveFilePath, false));
            SavedModelsList.Items.Add(model);
            WriteToLog("Model saved successfully!", null);
        }

        private void WriteToLog(string text, byte[] color)
        {
            if (color == null)
                color = new byte[] {0, 0, 0};

            TextLog.Inlines.Add(new Run($"{text}\n")
            {
                Foreground = new SolidColorBrush(Color.FromRgb(color[0], color[1], color[2]))
            });
        }

        private void ButtonInject_Click(object sender, RoutedEventArgs e)
        {
            Inject();

            TabEditModels.IsEnabled = true;
            TabInject.IsEnabled = false;
            ButtonInject.IsEnabled = false;

            _selectedProcess.Exited += HookedProcessExited;
            _selectedProcess.EnableRaisingEvents = true;

            TabLog.Focus();
        }

        private void HookedProcessExited(object sender, EventArgs e)
        {
            Dispatcher.Invoke(Close);
        }

        private void ProcessList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProcessList.SelectedItem == null)
            {
                ButtonInject.IsEnabled = false;
                TabEditModels.IsEnabled = false;
            }

            _selectedProcess = (Process) ProcessList.SelectedItem;
            ButtonInject.IsEnabled = true;
        }

        private void ButtonSaveChanges_Click(object sender, RoutedEventArgs e)
        {
            var models = SavedModelsList.Items.SourceCollection.Cast<ModelParameters>().ToList();
            
            SaveFile.Save(_saveFilePath, models);
            
            _serverInterface.ReloadModels(models.Where(x => x.Enabled).ToList());
        }

        private void CheckBoxEnable_Checked(object sender, RoutedEventArgs e)
        {
            ToggleModels();
        }

        private bool _toggleModels;
        private void ToggleModels()
        {
            _toggleModels = !_toggleModels;

            foreach (ModelParameters item in SavedModelsList.Items.SourceCollection)
            {
                item.Enabled = _toggleModels;
            }

            SavedModelsList.Items.Refresh();
        }
    }
}
