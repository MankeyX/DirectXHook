using System;
using System.Collections.Generic;
using System.Diagnostics;
using Core.Data;
using Core.Interop;
using Core.Models;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using UI.DirectX;
using UI.Models;
using UI.Remoting;

namespace UI.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        public string Log
        {
            get => Model.Log;
            set
            {
                Model.Log = value;
                RaisePropertyChanged();
            }
        }

        public List<ModelInfo> SavedModels
        {
            get => Model.SavedModels;
            set
            {
                Model.SavedModels = value;
                RaisePropertyChanged();
            }
        }

        private ModelInfo _selectedModel;
        public ModelInfo SelectedModel
        {
            get => _selectedModel;
            set
            {
                _selectedModel = value;
                RaisePropertyChanged();
            }
        }

        private int _tabSelectedIndex;
        public int TabSelectedIndex
        {
            get => _tabSelectedIndex;
            set
            {
                _tabSelectedIndex = value;
                RaisePropertyChanged();
            }
        }

        public Process SelectedProcess
        {
            get => Model.SelectedProcess;
            set
            {
                Model.SelectedProcess = value;
                RaisePropertyChanged();
                InjectCommand.RaiseCanExecuteChanged();
            }
        }

        private bool _tabInjectEnabled = true;
        public bool TabInjectEnabled
        {
            get => _tabInjectEnabled;
            set
            {
                _tabInjectEnabled = value;
                RaisePropertyChanged();
            }
        }

        private bool _tabEditModelsEnabled;
        public bool TabEditModelsEnabled
        {
            get => _tabEditModelsEnabled;
            set
            {
                _tabEditModelsEnabled = value;
                RaisePropertyChanged();
            }
        }

        public RelayCommand InjectCommand { get; }
        public RelayCommand DeleteSelectedModelCommand { get; }
        public RelayCommand SaveChangesCommand { get; }
        public RelayCommand ToggleModelsCommand { get; }
        public DxProcessMonitor DxProcessMonitor { get; }
        public MainWindowModel Model { get; }
        public HookManager HookManager { get; }
        public IModelInfoRepository ModelInfoRepository { get; }

        public MainViewModel()
        {
            Model = new MainWindowModel();
            ModelInfoRepository = new ModelInfoRepository();
            DxProcessMonitor = new DxProcessMonitor();

            InjectCommand = new RelayCommand(Inject, () => SelectedProcess != null);
            DeleteSelectedModelCommand = new RelayCommand(DeleteSelectedModel, () => SelectedModel != null);
            SaveChangesCommand = new RelayCommand(SaveChanges);
            ToggleModelsCommand = new RelayCommand(ToggleModels);
            
            DxProcessMonitor.Start();

            Model.WindowHandle = Process.GetCurrentProcess().MainWindowHandle;

            Messenger.Default.Register<NotificationMessage<ModelInfo>>(this, OnSaveModel);

            HookManager = new HookManager();
            HookManager.ServerInterface.MessageRecieved += WriteToLog;
            HookManager.ServerInterface.HookStarted += HookStarted;
            HookManager.ServerInterface.SaveModelRequestRecieved += SaveModelRequestRecieved;

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
            DispatcherHelper.RunAsync(() =>
            {
                try
                {
                    Messenger.Default.Send(modelInfo);
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

        private void OnSaveModel(NotificationMessage<ModelInfo> message)
        {
            if (message.Notification != "Save")
                return;
            
            SavedModels.Add(message.Content);
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