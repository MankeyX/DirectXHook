using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
using System.Threading;
using Core.Interop;
using Core.IO;
using Core.Models;
using EasyHook;
using Hook;
using UI.DirectX;
using UI.Models;
using UI.Windows;

namespace UI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public string Log
        {
            get { return Model.Log; }
            set
            {
                Model.Log = value;
                NotifyPropertyChanged();
            }
        }

        public List<ModelInfo> SavedModels
        {
            get
            {
                return Model.SavedModels;
            }
            set
            {
                Model.SavedModels = value;
                NotifyPropertyChanged();
            }
        }

        private int _tabSelectedIndex;
        public int TabSelectedIndex
        {
            get
            {
                return _tabSelectedIndex;
            }
            set
            {
                _tabSelectedIndex = value;
                NotifyPropertyChanged();
            }
        }
        
        public Process SelectedProcess
        {
            get
            {
                return Model.SelectedProcess;
            }
            set
            {
                Model.SelectedProcess = value;
                InjectCommand.RaiseCanExecuteChanged();
            }
        }

        private bool _tabInjectEnabled = true;
        public bool TabInjectEnabled
        {
            get
            {
                return _tabInjectEnabled;
            }
            set
            {
                _tabInjectEnabled = value;
                NotifyPropertyChanged();
            }
        }

        private bool _tabEditModelsEnabled;
        public bool TabEditModelsEnabled
        {
            get
            {
                return _tabEditModelsEnabled;
            }
            set
            {
                _tabEditModelsEnabled = value;
                NotifyPropertyChanged();
            }
        }

        private const string Filename = "savedModels"; // Repository

        public RelayCommand InjectCommand { get; }
        public RelayCommand SaveChangesCommand { get; }
        public RelayCommand ToggleModelsCommand { get; }
        public DxProcessMonitor DxProcessMonitor { get; }
        public MainWindowModel Model { get; }

        public MainWindowViewModel()
        {
            Model = new MainWindowModel();

            InjectCommand = new RelayCommand(Inject, () => SelectedProcess != null);
            SaveChangesCommand = new RelayCommand(SaveChanges);
            ToggleModelsCommand = new RelayCommand(ToggleModels);

            Model.SaveFilePath = Path.Combine(Directory.GetCurrentDirectory(), Filename);
            SavedModels = SaveLoad.Load(Model.SaveFilePath);

            DxProcessMonitor = new DxProcessMonitor(Dispatcher);
            DxProcessMonitor.Start();

            Model.WindowHandle = Process.GetCurrentProcess().MainWindowHandle;

            Model.SaveModelWindow = new SaveModelWindow();
            Model.SaveModelWindow.OnSaveModel += OnSaveModel;

            Model.ServerInterface = new ServerInterface();
            Model.ServerInterface.MessageRecieved += WriteToLog;
            Model.ServerInterface.HookStarted += HookStarted;
            Model.ServerInterface.SaveModelRequestRecieved += SaveModelRequestRecieved;

            WriteToLog("Controls:\n" +
                       "End = Toggle Model Logger\n" +
                       "\tNumpad 1 = Decrease Byte Width\n" +
                       "\tNumpad 3 = Increase Byte Width\n" +
                       "\tNumpad 2 = Previous Model\n" +
                       "\tNumpad 5 = Next Model\n" +
                       "\tInsert = Save Current Model\n" +
                       "Numpad 0 = Render Only Saved Models\n");
        }
        
        private void FocusWindow()
        {
            WindowManagement.ShowWindow(SelectedProcess.MainWindowHandle, WindowOptions.ForceMinimize);
            WindowManagement.ShowWindow(Model.WindowHandle, WindowOptions.Show);
        }

        private void Inject()
        {
            string channelName = null;

            Model.Server = RemoteHooking.IpcCreateServer(ref channelName, WellKnownObjectMode.Singleton, Model.ServerInterface);

            var injectionLibrary = Path.Combine(Directory.GetCurrentDirectory(), "Hook.dll");

            try
            {
                WriteToLog("Attempting to inject DLL into process...");

                RemoteHooking.Inject(
                    SelectedProcess.Id,
                    injectionLibrary,
                    injectionLibrary,
                    channelName
                    );

                DxProcessMonitor.Stop();

                Thread.Sleep(1000);

                TabEditModelsEnabled = true;
                TabInjectEnabled = false;
                TabSelectedIndex = 2;
            }
            catch (Exception e)
            {
                WriteToLog($"There was an error while injecting into target: {e}");
                TabSelectedIndex = 2;
            }
        }

        private void SaveModelRequestRecieved(ModelInfo modelInfo)
        {
            if (SaveLoad.Load(Model.SaveFilePath).Contains(modelInfo))
                return;
            
            WriteToLog($"Saving model...\n{modelInfo}\n");
            Dispatcher.Invoke(() =>
                Model.SaveModelWindow.Show(modelInfo));
            //_saveModelWindow.Owner = this;
            FocusWindow();
        }

        private void HookStarted()
        {
            Dispatcher.Invoke(() =>
                SavedModels = SaveLoad.Load(Model.SaveFilePath));
            Model.ServerInterface.ReloadModels(SavedModels.Where(x => x.Enabled).ToList());
        }

        private void OnSaveModel(ModelInfo modelInfo)
        {
            SaveLoad.Save(Model.SaveFilePath, modelInfo);
            Dispatcher.Invoke(() =>
                SavedModels = SaveLoad.Load(Model.SaveFilePath));
            Model.ServerInterface.ReloadModels(SavedModels.Where(x => x.Enabled).ToList());
            WriteToLog("Model info saved successfully!");
        }

        private void WriteToLog(string text)
        {
            Log += $"{text}\n";
        }

        private void SaveChanges()
        {
            SaveLoad.Save(Model.SaveFilePath, SavedModels);
            Model.ServerInterface.ReloadModels(SavedModels.Where(x => x.Enabled).ToList());
        }

        private bool _toggleModels;
        private void ToggleModels()
        {
            _toggleModels = !_toggleModels;

            foreach (var modelInfo in SavedModels)
            {
                modelInfo.Enabled = _toggleModels;
            }
        }
    }
}
