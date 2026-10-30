using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ProjectOrganization
{
    public class AmountToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal amount && amount > 2000000)
                return new SolidColorBrush(Color.FromRgb(200, 255, 200)); // Светло-зеленый

            return new SolidColorBrush(Colors.Transparent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}