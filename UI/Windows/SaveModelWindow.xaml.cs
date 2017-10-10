using System.ComponentModel;
using System.Windows;
using Core.Models;

namespace UI.Windows
{
    public delegate void OnSaveModelEvent(ModelInfo model);

    public partial class SaveModelWindow
    {
        public event OnSaveModelEvent OnSaveModel;

        private ModelInfo _model;

        public SaveModelWindow()
        {
            InitializeComponent();

            Closing += SaveModelWindow_Closing;
        }

        private void SaveModelWindow_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            TextBoxName.Clear();
            Visibility = Visibility.Hidden;
        }

        public void Show(ModelInfo model)
        {
            _model = model;
            LabelIndexCount.Content = model.IndexCount;
            LabelIndexByteWidth.Content = model.IndexByteWidth;
            LabelStride.Content = model.Stride;
            LabelVertexByteWidth.Content = model.VertexByteWidth;
            LabelTextureFormat.Content = model.Format;
            CheckBoxEnabled.IsChecked = true;

            Visibility = Visibility.Visible;
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            _model.Name = TextBoxName.Text;
            if (ColorPicker.SelectedColor != null)
                _model.Color = new Color(
                    ColorPicker.SelectedColor.Value.R,
                    ColorPicker.SelectedColor.Value.G, 
                    ColorPicker.SelectedColor.Value.B);
            else
                _model.Color = Color.White;
            _model.Enabled = CheckBoxEnabled.IsChecked ?? false;

            OnSaveModel?.Invoke(_model);
            Hide();
        }
    }
}
