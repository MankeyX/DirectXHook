using System.Windows;
using Core.Models;
using GalaSoft.MvvmLight.Messaging;

namespace UI.Windows
{
    public partial class SaveModelInfoWindow
    {
        public SaveModelInfoWindow()
        {
            InitializeComponent();
            Closing += OnClosing;

            Messenger.Default.Register<ModelInfo>(this, (modelInfo) =>
            {
                Show();
            });
            Messenger.Default.Register<NotificationMessage>(this, (message) =>
            {
                if (message.Notification == "Close")
                {
                    Close();
                }
            });
        }

        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Visibility = Visibility.Hidden;
        }
    }
}
