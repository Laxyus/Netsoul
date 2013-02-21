using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Serialization;
using NetsoulLib;
using NetsoulLib.Common;
using Windows.Storage;
using Windows.UI.Core;

namespace Windows8
{
    public class MainViewModel : INotifyPropertyChanged
    {
        CoreDispatcher Dispatch;
        public event PropertyChangedEventHandler PropertyChanged;

        private Netsoul NetSoul;
        public bool Connected { get; set; }

        public ICommand LogInCmd { get; set; }
        public string UserLogin { get; set; }

        public ICommand SendCmd { get; set; }

        public bool LoginBoxEnable { get; set; }

        public ObservableCollection<ContactInfo> ContactList { get; set; }

        public ContactInfo SelectedContact { get; set; }
        public int LastContactSelection { get; set; }

        public string ChatBox { get; set; }
        public string SendBox { get; set; }

        public ICommand AddContactCmd { get; set; }

        public MainViewModel(CoreDispatcher dispatcher)
        {
            this.Dispatch = dispatcher;
            this.NetSoul = null;
            this.Connected = false;
            this.LoginBoxEnable = true;
            this.ContactList = new ObservableCollection<ContactInfo>();
            this.LastContactSelection = -1;
            this.SelectedContact = null;
            this.ChatBox = String.Empty;
            this.SendBox = String.Empty;
            this.AddContactCmd = new MyCommand<string>(new Action<string>(arg =>
                {
                    bool check = false;
                    if (arg != String.Empty)
                    {
                        foreach (ContactInfo c in this.ContactList)
                        {
                            if (c.Login == arg)
                                check = true;
                        }
                        if (check == false)
                        {
                            this.ContactList.Add(new ContactInfo(arg));
                            this.NetSoul.AddContact(arg);
                            this.SaveContactList();
                        }
                    }
                    string test = String.Empty;
                    //foreach (string s in this.NetSoul.Verif)
                    //{
                    //    test += s;
                    //}
                    //if (this.NetSoul.ShutdownClient == true)
                    //    test += "shutdown";
                    this.ChatBox += test;
                    this.UpdateProperty("ChatBox");
                }));
            this.SendCmd = new MyCommand<string>(new Action<string>(arg =>
                {
                    if (this.SelectedContact.Socket > 0)
                    {
                        this.NetSoul.Send(this.SelectedContact.Socket, arg);
                      //  this.NetSoul.Message.Add(this.SelectedContact.Socket.ToString() + ":" + arg);
                        this.ChatBox += ">>> " + arg + "\n";
                        this.UpdateProperty("ChatBox");
                        this.SendBox = String.Empty;
                        this.UpdateProperty("SendBox");
                    }
                }));
        }

        public async void NetsoulConnection(string Server, string PortStr, string UsernameText, string PasswordText, string LocationText)
        {
            this.UserLogin = UsernameText;
            int Port = -1;
            Int32.TryParse(PortStr, out Port);
            if (this.Connected == false && Port != -1)
            {
                if (UsernameText.Length == 8 && UsernameText.Contains("_") && PasswordText.Length == 8)
                {
                    await this.LoadContactList(UsernameText);
                    this.UpdateProperty("ContactList");
                    this.NetSoul = new NetsoulRT();
                    //this.NetSoul.OnDataReceived += NetSoul_OnDataReceived;
                  //  this.NetSoul.OnContactUpdated += NetSoul_OnContactUpdated;
                    //this.NetSoul.OnErrorRaised += NetSoul_OnErrorRaised;
                    this.NetSoul.Login = UsernameText;
                    this.NetSoul.Password = PasswordText;
                    this.NetSoul.UserData = "WinRT-NetSoul";
                    if (LocationText != String.Empty)
                        this.NetSoul.UserLocation = LocationText;
                    bool ret = await this.NetSoul.ConnectAsync();
                    if (ret == true)
                    {
                        this.UpdateWatchList();
                        this.Connected = true;
                        this.UpdateProperty("Connected");
                        this.LoginBoxEnable = false;
                        this.UpdateProperty("LoginBoxEnable");
                        //this.NetSoul.StartServer();
                    }
                }
            }
        }

        //void NetSoul_OnErrorRaised(object sender, EventArgs e)
        //{
        //    foreach (string s in this.NetSoul.Errors)
        //    {
        //        NetsoulNotificationSystem.DisplayNotification(s, NetsoulNotificationType.Error);
        //    }
        //    this.NetSoul.Errors.Clear();
        //}

        private void UpdateWatchList()
        {
            List<string> Contacts = new List<string>();
            foreach (ContactInfo c in this.ContactList)
            {
                Contacts.Add(c.Login);
            }
           // this.NetSoul.AddContact(Contacts);
        }

        private async Task LoadContactList(string Login)
        {
            try
            {
                StorageFolder appData = await ApplicationData.Current.RoamingFolder.GetFolderAsync("MetroSoul");
                StorageFolder folder = await appData.GetFolderAsync(Login);
                try
                {
                    StorageFile file = await folder.GetFileAsync("contactlist.xml");
                    try
                    {
                        using (Stream str = await file.OpenStreamForReadAsync())
                        {
                            List<ContactInfo> Lst = new List<ContactInfo>();
                            XmlSerializer xs = new XmlSerializer(typeof(List<ContactInfo>));
                            Lst = (List<ContactInfo>)xs.Deserialize(str);
                            foreach (ContactInfo c in Lst)
                            {
                                this.ContactList.Add(new ContactInfo(c.Login));
                            }
                        }
                    }
                    catch
                    {
                        NetsoulNotificationSystem.DisplayNotification("Saved contact list can not be loaded", NetsoulNotificationType.Error);
                    }
                }
                catch
                {
                    NetsoulNotificationSystem.DisplayNotification("No saved list", NetsoulNotificationType.Error);
                }
            }
            catch
            {
            }
        }

        private async void SaveContactList()
        {
            try
            {
                StorageFolder appData = await ApplicationData.Current.RoamingFolder.CreateFolderAsync("MetroSoul", CreationCollisionOption.OpenIfExists);
                StorageFolder folder = await appData.CreateFolderAsync(this.UserLogin, CreationCollisionOption.OpenIfExists);
                try
                {
                    StorageFile file = await folder.CreateFileAsync("contactlist.xml", CreationCollisionOption.ReplaceExisting);
                    try
                    {
                        using (Stream str = await file.OpenStreamForWriteAsync())
                        {
                            XmlSerializer xs = new XmlSerializer(typeof(List<ContactInfo>));
                            List<ContactInfo> Lst = new List<ContactInfo>(this.ContactList);
                            xs.Serialize(str, Lst);
                        }
                    }
                    catch
                    {
                        NetsoulNotificationSystem.DisplayNotification("Contact list can not be saved", NetsoulNotificationType.Error);
                    }
                }
                catch
                {
                }
            }
            catch
            {
            }
        }

        async void NetSoul_OnContactUpdated(object sender, NetSoulContactUpdateEventArgs e)
        {
            //string[] update = e.UpdatedContact.Split(':');
            //switch (update[0])
            //{
            //    case "update":
            //        await this.UpdateContact(update);
            //        break;
            //    case "logout":
            //        this.LogOutContact(update);
            //        break;
            //    case "state":
            //        this.UpdateStateContact(update);
            //        break;
            //    default:
            //        break;
            //}
        }

        private async void UpdateStateContact(string[] update)
        {
            int socket = -1;
            if (Int32.TryParse(update[2], out socket))
                foreach (ContactInfo c in this.ContactList)
                {
                    if (c.Login == update[1] && c.Socket == socket)
                        if (this.Dispatch.HasThreadAccess == false)
                            await this.Dispatch.RunAsync(CoreDispatcherPriority.Normal, new DispatchedHandler(new Action(() =>
                            {
                                c.UpdateState(update[3]);
                            })));
                }
            this.UpdateProperty("ContactList");
        }

        private async void LogOutContact(string[] update)
        {
            int socket = -1;
            if (Int32.TryParse(update[2], out socket))
            {
                for (int i = 0; i < this.ContactList.Count; i++)
                {
                    if (this.ContactList[i].Login == update[1] && this.ContactList[i].Socket == socket)
                    {
                        if (this.Dispatch.HasThreadAccess == false)
                            await this.Dispatch.RunAsync(CoreDispatcherPriority.Normal, new DispatchedHandler(new Action(() =>
                            {
                                if (this.ContactList[i].Clone == true)
                                    this.ContactList.RemoveAt(i);
                                else
                                {
                                    this.ContactList[i].Logout();
                                }
                            })));
                        break;
                    }
                }
            }
            this.UpdateProperty("ContactList");
        }

        private async Task UpdateContact(string[] update)
        {
            bool found = false;
            int socket;
            if (Int32.TryParse(update[5], out socket))
            {
                for (int i = 0; i < this.ContactList.Count; i++)
                {
                    if (this.ContactList[i].Login == update[1])
                    {
                        if (this.ContactList[i].Socket == socket || this.ContactList[i].Socket == -1)
                        {
                            if (this.Dispatch.HasThreadAccess == false)
                                await this.Dispatch.RunAsync(CoreDispatcherPriority.Normal, new DispatchedHandler(new Action(() =>
                                {
                                    this.ContactList[i].Update(socket, update[3], update[4], update[2]);
                                })));
                            found = true;
                            break;
                        }
                    }
                }
                if (found == false)
                    if (this.Dispatch.HasThreadAccess == false)
                        await this.Dispatch.RunAsync(CoreDispatcherPriority.Normal, new DispatchedHandler(new Action(() =>
                        {
                            this.ContactList.Add(new ContactInfo(update[1], update[3], update[4], update[2], socket));
                        })));
            }
            this.UpdateProperty("ContactList");
        }

        //private async void NetSoul_OnDataReceived(object sender, NetSoulDataEventArgs e)
        //{
        //    string tmp = String.Empty;
        //    bool found = false;
        //    int socket = -1;
        //    while (this.NetSoul.Data.Count > 0)
        //    {
        //        tmp = this.NetSoul.Data[0];
        //        if (this.Dispatch.HasThreadAccess == false)
        //            await this.Dispatch.RunAsync(CoreDispatcherPriority.Normal, new DispatchedHandler(new Action(() =>
        //            {
        //                NetsoulNotificationSystem.DisplayNotification("From " + tmp.Split(':')[1] + " at " + StringToUrlConverter.Convert(tmp.Split(':')[3], ConverterMode.UrlToStandard), NetsoulNotificationType.Message);
        //            })));
        //        Int32.TryParse(tmp.Split(':')[2], out socket);
        //        if (this.SelectedContact != null && tmp.Split(':')[1] == this.SelectedContact.Login && tmp.Split(':')[2] == this.SelectedContact.Socket.ToString())
        //        {
        //            this.ChatBox += tmp.Split(':')[4] + "\n";
        //            this.UpdateProperty("ChatBox");
        //        }
        //        else
        //        {
        //            for (int i = 0; i < this.ContactList.Count; i++)
        //            {
        //                if (tmp.Split(':')[1] == this.ContactList[i].Login && tmp.Split(':')[2] == this.ContactList[i].Socket.ToString())
        //                {
        //                    this.ContactList[i].ConversationLog += tmp.Split(':')[4] + "\n";
        //                    found = true;
        //                }
        //            }
        //            if (found == false)
        //            {
        //                if (this.Dispatch.HasThreadAccess == false)
        //                    await this.Dispatch.RunAsync(CoreDispatcherPriority.Normal, new DispatchedHandler(new Action(() =>
        //                        {
        //                            this.ContactList.Add(new ContactInfo(tmp.Split(':')[1], "unknown", "unknown", "none", socket));
        //                            this.ContactList.Last().ConversationLog += tmp.Split(':')[4] + "\n";
        //                        })));
        //            }
        //        }
        //        this.NetSoul.Data.RemoveAt(0);
        //    }
        //    this.UpdateProperty("ContactList");
        //}

        public void ContactListSelectionChanged(int idx)
        {
            if (this.ChatBox != String.Empty && this.LastContactSelection >= 0 && this.LastContactSelection < this.ContactList.Count)
                this.ContactList[this.LastContactSelection].ConversationLog = this.ChatBox;
            if (idx >= 0 && idx < this.ContactList.Count)
                this.SelectedContact = this.ContactList[idx];
            else
                this.SelectedContact = null;
            this.UpdateProperty("SelectedContact");
            this.UpdateChatBox(idx);
        }

        private void UpdateChatBox(int idx)
        {
            if (idx >= 0 && idx < this.ContactList.Count)
                this.ChatBox = this.ContactList[idx].ConversationLog;
            else
                this.ChatBox = String.Empty;
            this.LastContactSelection = idx;
            this.UpdateProperty("ChatBox");
        }

        private async void UpdateProperty(string propertyName)
        {
            if (this.Dispatch.HasThreadAccess == false)
                await this.Dispatch.RunAsync(CoreDispatcherPriority.Normal, new DispatchedHandler(new Action(() =>
                    {
                        if (this.PropertyChanged != null)
                            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                    })));
            else
                if (this.PropertyChanged != null)
                    this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
