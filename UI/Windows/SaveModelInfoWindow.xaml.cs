using System.Windows;
using Core.Models;
using CommunityToolkit.Mvvm.Messaging;
using UI.Messages;

namespace UI.Windows
{
    public partial class SaveModelInfoWindow
    {
        public SaveModelInfoWindow()
        {
            InitializeComponent();
            Closing += OnClosing;

            WeakReferenceMessenger.Default.Register<SaveModelInfoWindow, ModelInfo>(this, (recipient, modelInfo) =>
            {
                recipient.Show();
            });
            WeakReferenceMessenger.Default.Register<SaveModelInfoWindow, CloseSaveModelWindowMessage>(this, (recipient, message) =>
            {
                recipient.Close();
            });
        }

        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Visibility = Visibility.Hidden;
        }
    }
}
