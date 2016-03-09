using System;
using System.Globalization;
using System.Windows.Data;

namespace PSEGet3.View.ValueConverters
{
    public class NegativeValueToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double val = 0;
            if (value != null)
            {
                val = (double) value;
            }

            if (val < 0)
                return val.ToString((string) parameter).Remove(0, 1) + "-";
            return val.ToString((string) parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}