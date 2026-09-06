using System;
using System.Globalization;
using System.Windows.Data;

namespace User.ActiveBeltTensioner
{
    /// <summary>Converts between an integer in the range 0-1000 (or -1000-1000) and a double in the range 0-100 (or -100-100)</summary>
    public class ThousandthsToPercentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue)
            {
                return intValue / 10.0;
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double doubleValue)
            {
                return (int)Math.Round(doubleValue * 10.0);
            }

            return value;
        }
    }
}
