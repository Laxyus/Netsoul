using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace Windows8
{
    public enum NetsoulNotificationType
    {
        Error,
        Notification,
        Message
    }

    public class NetsoulNotificationSystem
    {

        static public void DisplayNotification(string message, NetsoulNotificationType type)
        {
            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02);

            XmlNodeList textElements = toastXml.GetElementsByTagName("text");
            switch (type)
            {
                case NetsoulNotificationType.Error:
                    textElements.Item(0).AppendChild(toastXml.CreateTextNode("Error"));
                    break;
                case NetsoulNotificationType.Notification:
                    textElements.Item(0).AppendChild(toastXml.CreateTextNode("Notification"));
                    break;
                case NetsoulNotificationType.Message:
                    textElements.Item(0).AppendChild(toastXml.CreateTextNode("New Message"));
                    break;
                default:
                    break;
            }
            textElements.Item(1).AppendChild(toastXml.CreateTextNode(message));
            ToastNotification toast = new ToastNotification(toastXml);
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }
    }
}
