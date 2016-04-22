using System;
using System.Globalization;
using Xamarin.Forms;

namespace Philosopher.Multiplat.Converters
{
    public class TrueIfGrEqualConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {            
            if (parameter == null)
            {
                return true;
            }            
            double v = (double)value;
            double p;
            if (parameter is GridLength)
            {
                p = ((GridLength) parameter).Value;
            }
            else
            {
                p = Double.Parse(parameter.ToString());
            }
            
            return v >= p;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}