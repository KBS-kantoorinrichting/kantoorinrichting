﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace Designer.Utils {
    public class PriceConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return $"€{value:N2}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}