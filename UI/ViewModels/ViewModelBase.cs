using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;
using UI.Annotations;

namespace UI.ViewModels
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected Dispatcher Dispatcher { get; }

        public ViewModelBase()
        {
            Dispatcher = Dispatcher.CurrentDispatcher;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
