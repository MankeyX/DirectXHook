using Core.Models;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;

namespace UI.ViewModel
{
    public class SaveModelInfoViewModel : ViewModelBase
    {
        private ModelInfo _model;
        public ModelInfo Model
        {
            get { return _model; }
            set
            {
                _model = value;
                RaisePropertyChanged();
            }
        }

        public string ModelName
        {
            get { return Model.Name; }
            set
            {
                Model.Name = value;
                RaisePropertyChanged();
                SaveCommand.RaiseCanExecuteChanged();
            }
        }

        public RelayCommand SaveCommand { get; set; }

        public SaveModelInfoViewModel()
        {
            SaveCommand = new RelayCommand(Save, () => !string.IsNullOrEmpty(Model?.Name));
            Messenger.Default.Register<ModelInfo>(this, (modelInfo) =>
            {
                Model = modelInfo;
                ModelName = "";
            });
        }

        private void Save()
        {
            Messenger.Default.Send(new NotificationMessage<ModelInfo>(Model, "Save"));
            Messenger.Default.Send(new NotificationMessage("Close"));
        }
    }
}
