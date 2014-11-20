using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace ProcessMQTT.WPF.ValueConverters
{
    public class TopicToBackgroundColourValueConverter : IValueConverter
    {
        private static Color[] colours = new Color[] 
        { 
            Colors.Red,
            Colors.Green,
            Colors.Blue,
            Colors.Orange,
            Colors.Yellow,
            Colors.Purple,
            Colors.Magenta,
            Colors.Cyan,
            Colors.DarkBlue,
            Colors.DarkKhaki,
            Colors.DarkOrange,
            Colors.Gold,
            Colors.Green,
            Colors.LawnGreen,
            Colors.LimeGreen,
            Colors.Magenta,
            Colors.Moccasin,
            Colors.Olive,
            Colors.Plum,
            Colors.Salmon,
            Colors.SkyBlue,
            Colors.Tan,
            Colors.Teal,
            Colors.Turquoise,
            Colors.YellowGreen
        };
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // default brush is white
            Brush bg = new SolidColorBrush(Colors.White);

            // calculate a hash based on the topic passed
            var topic = value as string;
            if(topic != null)
            {
                // topic in the form gb/emul/light
                // get everything up to the last forward slash
                var lastSlashPos = topic.LastIndexOf('/');
                string machine = topic.Substring(0, lastSlashPos + 1);
                // calculate a numeric hash
                var hash = machine.GetHashCode();
                // take a mod (number of elements in the colours array)
                // need abs as it's a signed int, and the hash is sometimes negative
                var colourIndex = Math.Abs(hash % colours.Length);
                // use that colour
                bg = new SolidColorBrush(colours[colourIndex]);
            }

            bg.Opacity = 0.4;
            return bg;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
