using System;
using System.Globalization;
using System.Windows.Data;

namespace PSEGet3.View.ValueConverters
{
    public class DirectionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                var val = (double) value;
                if (val > 0)
                    return 1;
                if (val < 0)
                    return -1;
                return 0;
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}