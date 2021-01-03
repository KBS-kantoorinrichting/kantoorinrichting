using System;
using System.Globalization;
using System.Windows.Data;

namespace Designer.Utils {
    public class ImagePathConverter : IValueConverter {
        //Deze class wordt gebruikt overal om het de bestandsnaam in de database
        //om te zetten naar een relatief pad voor o.a. de product-catalogus
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return string.Format("{1}/Resources/Images/{0}", value, Environment.CurrentDirectory);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}