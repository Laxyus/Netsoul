using System;

namespace NetsoulLib.Common
{
    public class NetSoulContactUpdateEventArgs : EventArgs
    {
        public string Login { get; set; }
        public ContactStatus Status { get; set; }
        public string Location { get; set; }
        public string UserData { get; set; }
        public int UserSocket { get; set; }

        public NetSoulContactUpdateEventArgs(string login, ContactStatus status, string location, string userData, string userSocket)
        {
            this.Login = login;
            this.Status = status;
            this.Location = location;
            this.UserData = UserData;
            this.UserSocket = int.Parse(userSocket);
        }
    }
}
