using System.Globalization;
using System.Windows.Controls;

namespace Designer.Utils
{
    public class ObjectNotNull : ValidationRule
    {
            public string ObjectName { get; set; }
            public string FieldName { get; set; }

            public override ValidationResult Validate(object value, CultureInfo cultureInfo)
            {
                if (value == null)
                    return new ValidationResult(false, $"Een {ObjectName} moet een {FieldName} hebben");
                else
                    return ValidationResult.ValidResult;
            }
    }
}