using System;
using System.Globalization;
using System.Windows.Data;
using PSEGetLib;

namespace PSEGet3.View.ValueConverters
{
    public class OutputToConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;
            string checkValue = value.ToString();
            string targetValue = parameter.ToString();
            bool result = checkValue.Equals(targetValue, StringComparison.InvariantCultureIgnoreCase);
            if (checkValue == "Amibroker" && !Helpers.IsAmibrokerInstalled())
                result = false;
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return null;
            var useValue = (bool) value;
            string targetValue = parameter.ToString();
            if (useValue)
                return Enum.Parse(targetType, targetValue);

            return null;
        }
    }
}