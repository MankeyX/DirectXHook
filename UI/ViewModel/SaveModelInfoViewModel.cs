using Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using UI.Messages;

namespace UI.ViewModel
{
    public class SaveModelInfoViewModel : ObservableObject
    {
        private ModelInfo _model;
        public ModelInfo Model
        {
            get => _model;
            set
            {
                _model = value;
                OnPropertyChanged();
            }
        }

        public string ModelName
        {
            get => Model.Name;
            set
            {
                Model.Name = value;
                OnPropertyChanged();
                SaveCommand.NotifyCanExecuteChanged();
            }
        }

        public RelayCommand SaveCommand { get; set; }

        public SaveModelInfoViewModel()
        {
            SaveCommand = new RelayCommand(Save, () => !string.IsNullOrEmpty(Model?.Name));
            WeakReferenceMessenger.Default.Register<SaveModelInfoViewModel, ModelInfo>(this, (recipient, modelInfo) =>
            {
                recipient.Model = modelInfo;
                recipient.ModelName = "";
            });
        }

        private void Save()
        {
            WeakReferenceMessenger.Default.Send(new SaveModelMessage(Model));
            WeakReferenceMessenger.Default.Send(new CloseSaveModelWindowMessage());
        }
    }
}
