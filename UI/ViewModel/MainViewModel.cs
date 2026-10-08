using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using Core.Data;
using Core.Interop;
using Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using UI.DirectX;
using UI.Messages;
using UI.Models;
using UI.Remoting;

namespace UI.ViewModel
{
    public class MainViewModel : ObservableObject
    {
        public string Log
        {
            get => Model.Log;
            set
            {
                Model.Log = value;
                OnPropertyChanged();
            }
        }

        public List<ModelInfo> SavedModels
        {
            get => Model.SavedModels;
            set
            {
                Model.SavedModels = value;
                OnPropertyChanged();
            }
        }

        private ModelInfo _selectedModel;
        public ModelInfo SelectedModel
        {
            get => _selectedModel;
            set
            {
                _selectedModel = value;
                OnPropertyChanged();
            }
        }

        private int _tabSelectedIndex;
        public int TabSelectedIndex
        {
            get => _tabSelectedIndex;
            set
            {
                _tabSelectedIndex = value;
                OnPropertyChanged();
            }
        }

        public Process SelectedProcess
        {
            get => Model.SelectedProcess;
            set
            {
                Model.SelectedProcess = value;
                OnPropertyChanged();
                InjectCommand.NotifyCanExecuteChanged();
            }
        }

        private bool _tabInjectEnabled = true;
        public bool TabInjectEnabled
        {
            get => _tabInjectEnabled;
            set
            {
                _tabInjectEnabled = value;
                OnPropertyChanged();
            }
        }

        private bool _tabEditModelsEnabled;
        public bool TabEditModelsEnabled
        {
            get => _tabEditModelsEnabled;
            set
            {
                _tabEditModelsEnabled = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand InjectCommand { get; }
        public RelayCommand DeleteSelectedModelCommand { get; }
        public RelayCommand SaveChangesCommand { get; }
        public RelayCommand ToggleModelsCommand { get; }
        public DxProcessMonitor DxProcessMonitor { get; }
        public MainWindowModel Model { get; }
        public HookManager HookManager { get; }
        public IModelInfoRepository ModelInfoRepository { get; private set; }

        public MainViewModel()
        {
            Model = new MainWindowModel();
            DxProcessMonitor = new DxProcessMonitor();

            InjectCommand = new RelayCommand(Inject, () => SelectedProcess != null);
            DeleteSelectedModelCommand = new RelayCommand(DeleteSelectedModel, () => SelectedModel != null);
            SaveChangesCommand = new RelayCommand(SaveChanges);
            ToggleModelsCommand = new RelayCommand(ToggleModels);
            
            DxProcessMonitor.Start();

            Model.WindowHandle = Process.GetCurrentProcess().MainWindowHandle;

            WeakReferenceMessenger.Default.Register<MainViewModel, SaveModelMessage>(this, (recipient, message) => recipient.OnSaveModel(message));

            HookManager = new HookManager();
            HookManager.MessageRecieved += WriteToLog;
            HookManager.HookStarted += HookStarted;
            HookManager.SaveModelRequestRecieved += SaveModelRequestRecieved;

            var is64bit = Environment.Is64BitProcess ? "64-bit" : "32-bit";
            var is64bitOs = Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit";
            WriteToLog($"App target: {is64bit}, OS: {is64bitOs}");
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
            try
            {
                WriteToLog("Attempting to inject DLL into process...");

                ModelInfoRepository = new ModelInfoRepository($"{SelectedProcess.ProcessName}.json");

                HookManager.Inject(SelectedProcess.Id);
                DxProcessMonitor.Stop();

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
            if (ModelInfoRepository.Get().Contains(modelInfo))
                return;

            WriteToLog($"Saving model...\n{modelInfo}\n");
            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    WeakReferenceMessenger.Default.Send(modelInfo);
                }
                catch (Exception e)
                {
                    WriteToLog(e.ToString());
                }
            });

            FocusWindow();
        }

        private void HookStarted()
        {
            SavedModels = ModelInfoRepository.Get();
            HookManager.ReloadModels(SavedModels);
        }

        private void OnSaveModel(SaveModelMessage message)
        {
            SavedModels.Add(message.Value);
            SaveChanges();
            
            SavedModels = ModelInfoRepository.Get();

            WriteToLog("Model info saved successfully!");
        }

        private void WriteToLog(string text)
        {
            Log += $"{text}\n";
        }

        private void DeleteSelectedModel()
        {
            SavedModels.Remove(SelectedModel);
            ModelInfoRepository.Save(SavedModels);
            SavedModels = ModelInfoRepository.Get();
            HookManager.ReloadModels(SavedModels);
        }

        private void SaveChanges()
        {
            ModelInfoRepository.Save(SavedModels);
            HookManager.ReloadModels(SavedModels);
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