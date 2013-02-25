using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetsoulLib.Common
{
    public interface INetsoul
    {
        string Server { get; set; }
        int ServerPort { get; set; }
        string Login { get; set; }
        string Password { get; set; }
        string UserLocation { get; set; }
        string UserData { get; set; }
        bool IsLogued { get; }

        event EventHandler<NetsoulMessageEventArgs> OnMessage;
        event EventHandler<NetSoulContactUpdateEventArgs> OnContactUpdate;

        Task<bool> ConnectAsync();

        Task<bool> Send(int userSocket, string message);

        Task<bool> RefreshContact(string contact);
        Task<bool> RefreshContacts(List<string> contacts);

        Task<bool> AddContact(string contact);
        Task<bool> AddContacts(List<string> contacts);
    }
}
