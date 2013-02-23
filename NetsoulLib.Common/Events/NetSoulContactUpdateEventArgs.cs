using System;

namespace NetsoulLib.Common
{
    public class NetSoulContactUpdateEventArgs : EventArgs
    {
        public Contact Contact { get; set; }

        public NetSoulContactUpdateEventArgs(string login, ContactStatus status, string location, string userData, string userSocket)
        {
            this.Contact = new Contact(login, status, location, userData, userSocket);
        }
    }
}
