using System;
using System.Globalization;
using System.Windows.Data;
using PSEGetLib;
using PSEGetLib.Interfaces;
using Microsoft.Practices.ServiceLocation;

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

            var amibrokerService = ServiceLocator.Current.GetInstance<IAmibrokerService>();
            if (checkValue == "Amibroker" && !amibrokerService.IsAmibrokerInstalled())
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