using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetsoulLib;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Windows8
{
    public class StatusToColorConverter : IValueConverter
    { // Ne Fonctionne pas >> Ne se met pas a jour en cas de modification
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            SolidColorBrush clr = null;
            switch ((ContactStatus)value)
            {
                case ContactStatus.Online:
                    clr = new SolidColorBrush(Colors.Green);
                    break;
                case ContactStatus.Away:
                    clr = new SolidColorBrush(Colors.Yellow);
                    break;
                case ContactStatus.Connection:
                    clr = new SolidColorBrush(Colors.Blue);
                    break;
                case ContactStatus.Idle:
                    clr = new SolidColorBrush(Colors.Gray);
                    break;
                case ContactStatus.Lock:
                    clr = new SolidColorBrush(Colors.Purple);
                    break;
                case ContactStatus.None:
                    clr = new SolidColorBrush(Colors.Black);
                    break;
                case ContactStatus.Server:
                    clr = new SolidColorBrush(Colors.Black);
                    break;
                case ContactStatus.Offline:
                    clr = new SolidColorBrush(Colors.Red);
                    break;
                default:
                    clr = new SolidColorBrush(Colors.Black);
                    break;
            }
            return clr;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
