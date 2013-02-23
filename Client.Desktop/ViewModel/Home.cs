using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Desktop.Helpers;
using NetsoulLib.Common;
using NetsoulLib.Desktop;

namespace Desktop.ViewModel
{
    public class Home : ViewModelBase
    {
        private INetsoul netsoul;
        private Dispatcher disp;

        private bool isLogued;
        public bool IsLogued
        {
            get
            {
                return this.isLogued;
            }
            set
            {
                this.SetProperty(ref this.isLogued, value, "IsLogued");
            }
        }

        public ICommand LoadedCMD { get; set; }

        public ObservableCollection<Contact> Contacts { get; set; }

        public Home()
        {
            this.disp = App.Current.MainWindow.Dispatcher;
            this.Contacts = new ObservableCollection<Contact>();

            this.LoadedCMD = new RelayCommand(OnLoaded);
        }

        private async void OnLoaded()
        {
            this.netsoul = new NetsoulDesktop();
            this.netsoul.OnContactUpdate += netsoul_OnContactUpdate;

            bool designTime = System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject());
            if (designTime == false)
            {
                await this.Connect();
                await this.LoadFriendList();

                this.Contacts.Add(new Contact());
            }
        }

        private async Task LoadFriendList()
        {
            await this.netsoul.AddContact("freier_n");
        }

        private void netsoul_OnContactUpdate(object sender, NetSoulContactUpdateEventArgs e)
        {
            var contact = this.Contacts.Where(c => c.Login == e.Contact.Login).FirstOrDefault();
            if (contact != null)
            {
                if (e.Contact.Status == ContactStatus.Offline)
                {
                    this.Contacts.Remove(contact);
                }
                else
                {
                    contact.Status = e.Contact.Status;
                    contact.Location = e.Contact.Location;
                }
            }
            else
            {
                if (e.Contact.Status != ContactStatus.Offline)
                {
                    var c = new Contact();
                    c.Login = e.Contact.Login;
                    c.Location = e.Contact.Location;
                    c.Status = e.Contact.Status;
                    c.UserData = e.Contact.UserData;
                    c.UserSocket = e.Contact.UserSocket;
                    this.disp.Invoke(() =>
                        {
                            Console.WriteLine("fff");
                            this.Contacts.Add(c);
                            Console.WriteLine("ddd");
                        });
                }
            }
        }

        private async Task Connect()
        {
            this.netsoul.Login = "freier_n";
            this.netsoul.Password = "a3L[aDn5";
            this.IsLogued = await this.netsoul.ConnectAsync();
        }
    }
}
