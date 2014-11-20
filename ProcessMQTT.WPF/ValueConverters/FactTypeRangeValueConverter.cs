using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ProcessMQTT.WPF.ValueConverters {
    public class FactTypeRangeValueConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            var inputType = value as string;
            if (string.IsNullOrWhiteSpace(inputType))
                return 0d;

            var maxMin = (parameter as string) ?? string.Empty;
            double answer;
            switch (inputType.ToLowerInvariant()) {
                case "temp":
                    if (maxMin.ToLowerInvariant() == "max")
                        answer = 50d;
                    else
                        answer = 10d;
                    break;
                case "humd":
                    if (maxMin.ToLowerInvariant() == "max")
                        answer = 100d;
                    else
                        answer = 0d;
                    break;
                case "sound":
                    if (maxMin.ToLowerInvariant() == "max")
                        answer = 250;
                    else
                        answer = 0;
                    break;
                case "ftsnd":
                    if (maxMin.ToLowerInvariant() == "max")
                        answer = 600;
                    else
                        answer = 200;
                    break;
                case "light":
                    if (maxMin.ToLowerInvariant() == "max")
                        answer = 100d;
                    else
                        answer = 0d;
                    break;
                case "mem":
                    if (maxMin.ToLowerInvariant() == "max")
                        answer = 120d;
                    else
                        answer = 0d;
                    break;
                case "error":
                    if (maxMin.ToLowerInvariant() == "max")
                        answer = 200;
                    else
                        answer = 0;
                    break;
                default:
                    if (maxMin.ToLowerInvariant() == "max")
                        answer = 500;
                    else
                        answer = 0;
                    break;
            }

            return answer;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
