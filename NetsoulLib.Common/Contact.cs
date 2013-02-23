using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetsoulLib.Common
{
    public class Contact
    {
        public string Login { get; set; }
        public ContactStatus Status { get; set; }
        public string Location { get; set; }
        public string UserData { get; set; }
        public int UserSocket { get; set; }

        public Contact()
        {
        }

        public Contact(string login, ContactStatus status, string location, string userData, string userSocket)
        {
            this.Login = login;
            this.Status = status;
            this.Location = location;
            this.UserData = UserData;
            this.UserSocket = int.Parse(userSocket);
        }
    }
}
