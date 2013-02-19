using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace NetsoulLib
{
    public enum ContactStatus
    {
        Online,
        Away,
        Connection,
        Idle,
        Lock,
        None,
        Server,
        Offline
    }

    public class ContactInfo
    {
        public string Login { get; set; }
        [XmlIgnore]
        public string Location { get; set; }
        [XmlIgnore]
        public string UserData { get; set; }
        [XmlIgnore]
        public string Host { get; set; }
        [XmlIgnore]
        public ContactStatus Status { get; set; }
        [XmlIgnore]
        public int Socket { get; set; }
        [XmlIgnore]
        public string ConversationLog { get; set; }
        [XmlIgnore]
        public bool Clone { get; set; }
        [XmlIgnore]
        public SolidColorBrush Color { get; set; }

        public ContactInfo()
        {
            this.Login = String.Empty;
            this.Location = "none";
            this.UserData = "none";
            this.Host = "Unknown";
            this.Status = ContactStatus.Offline;
            this.Socket = -1;
            this.ConversationLog = String.Empty;
            this.Clone = false;
            this.Color = new SolidColorBrush(Colors.Red);
        }

        public ContactInfo(string login)
        {
            this.Login = login;
            this.Location = "none";
            this.UserData = "none";
            this.Host = "Unknown";
            this.Status = ContactStatus.Offline;
            this.Socket = -1;
            this.ConversationLog = String.Empty;
            this.Clone = false;
            this.Color = new SolidColorBrush(Colors.Red);
        }

        public ContactInfo(string login, string loc, string data, string status, int socket)
        {
            this.Login = login;
            this.Location = loc;
            this.UserData = data;
            this.UpdateState(status);
            this.Socket = socket;
            this.ConversationLog = String.Empty;
            this.Clone = true;
        }

        public void Update(int socket, string loc, string data, string status)
        {
            this.Location = loc;
            this.UserData = data;
            this.Socket = socket;
            this.UpdateState(status);
        }

        public void Logout()
        {
            this.Location = "none";
            this.UserData = "none";
            this.Host = "Unknown";
            this.Status = ContactStatus.Offline;
            this.Socket = -1;
        }

        public void UpdateState(string status)
        {
            if (this.Color == null)
                this.Color = new SolidColorBrush();
            switch (status)
            {
                case "actif":
                    this.Status = ContactStatus.Online;
                    break;
                case "away":
                    this.Status = ContactStatus.Away;
                    break;
                case "connection":
                    this.Status = ContactStatus.Connection;
                    break;
                case "idle":
                    this.Status = ContactStatus.Idle;
                    break;
                case "lock":
                    this.Status = ContactStatus.Lock;
                    break;
                case "server":
                    this.Status = ContactStatus.Server;
                    break;
                case "none":
                    this.Status = ContactStatus.None;
                    break;
                default:
                    break;
            }
            this.UpdateColor();
        }

        private void UpdateColor()
        {
            switch (this.Status)
            {
                case ContactStatus.Online:
                    this.Color.Color = Colors.Green;
                    break;
                case ContactStatus.Away:
                    this.Color.Color = Colors.Orange;
                    break;
                case ContactStatus.Connection:
                    this.Color.Color = Colors.Blue;
                    break;
                case ContactStatus.Idle:
                    this.Color.Color = Colors.Gray;
                    break;
                case ContactStatus.Lock:
                    this.Color.Color = Colors.Magenta;
                    break;
                case ContactStatus.None:
                    this.Color.Color = Colors.Black;
                    break;
                case ContactStatus.Server:
                    this.Color.Color = Colors.Black;
                    break;
                case ContactStatus.Offline:
                    this.Color.Color = Colors.Red;
                    break;
                default:
                    this.Color.Color = Colors.Black;
                    break;
            }
        }
    }
}
