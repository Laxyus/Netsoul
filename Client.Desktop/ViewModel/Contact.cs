using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetsoulLib.Common;

namespace Desktop.ViewModel
{
    public class Contact : ViewModelBase
    {
        private string login;
        public string Login
        {
            get
            {
                return this.login;
            }
            set
            {
                this.SetProperty(ref this.login, value, "Login");
            }
        }

        private string location;
        public string Location
        {
            get
            {
                return this.location;
            }
            set
            {
                this.SetProperty(ref this.location, value, "Location");
            }
        }

        private ContactStatus status;
        public ContactStatus Status
        {
            get
            {
                return this.status;
            }
            set
            {
                this.SetProperty(ref this.status, value, "Status");
            }
        }

        private string userData;
        public string UserData
        {
            get
            {
                return this.userData;
            }
            set
            {
                this.SetProperty(ref this.userData, value, "UserData");
            }
        }

        private int userSocket;
        public int UserSocket
        {
            get
            {
                return this.userSocket;
            }
            set
            {
                this.SetProperty(ref this.userSocket, value, "UserSocket");
            }
        }

        public Contact()
        { }

        public Contact(NetsoulLib.Common.Contact contact)
        {
            this.Login = contact.Login;
            this.Location = contact.Location;
            this.Status = contact.Status;
            this.UserData = contact.UserData;
            this.UserSocket = contact.UserSocket;
        }
    }
}
