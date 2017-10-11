using System;

namespace UI.Windows
{
    public partial class MainWindow
    {
        private SaveModelInfoWindow SaveModelInfoView { get; }

        public MainWindow()
        {
            InitializeComponent();

            SaveModelInfoView = new SaveModelInfoWindow();
            SaveModelInfoView.Hide();
        }

        protected override void OnActivated(EventArgs e)
        {
            SaveModelInfoView.Owner = this;
        }
    }
}
