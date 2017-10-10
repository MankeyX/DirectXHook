using System;
using System.Globalization;
using System.Windows.Data;
using Core.Models;

namespace UI
{
    public class ColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var color = value as Color;

            return color == null 
                ? System.Windows.Media.Color.FromRgb(255, 255, 255) 
                : System.Windows.Media.Color.FromRgb(color.R, color.G, color.B);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var color = value as System.Windows.Media.Color? ?? System.Windows.Media.Color.FromRgb(255, 255, 255);

            return new Color(color.R, color.G, color.B);
        }
    }
}
