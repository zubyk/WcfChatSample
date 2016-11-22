using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using WcfChatSample.Client.Wpf.ServiceReference;

namespace WcfChatSample.Client.Wpf.Converters
{
    internal class ChatMessageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var msg = value as ChatMessage;

            if (msg != null)
            {
                switch((int)parameter)
                {
                    case 0:
                        return String.Format("[{0:H:mm:ss}]",  msg.Date);
                    case 1:
                        return !String.IsNullOrWhiteSpace(msg.Username) ? String.Format(" {0}",  msg.Username) : "";
                    case 2:
                        return String.Format("[{0:H:mm:ss}]", msg.Date);
                    case 3:
                        return String.Format("[{0:H:mm:ss}]", msg.Date);
                }
                
                return String.Format("[{0:H:mm:ss}]{1}: {2}", msg.Date, msg.Username != null ? " <Bold>" + msg.Username + "</Bold>" : null, msg.Text);
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
