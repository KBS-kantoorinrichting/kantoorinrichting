using System.Globalization;
using System.Windows.Controls;
using Models;

namespace Designer.Utils
{
    public class StringNotEmpty : ValidationRule
    {
        public string ObjectName { get; set; }
        public string FieldName { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value == null || value as string == "")
                return new ValidationResult(false, $"Een {ObjectName} kan geen lege {FieldName} hebben");
            else
                return ValidationResult.ValidResult;
        }
    }
}