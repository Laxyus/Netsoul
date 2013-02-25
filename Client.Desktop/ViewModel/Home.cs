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
        private const string imagePath = "https://www.epitech.eu/intra/photos/{0}.jpg";

        private INetsoul netsoul;
        private Dispatcher disp;
        private string login;
        private int selectedContact = -1;
        private int selectedStatus = 0;

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

        public int SelectedContact
        {
            get
            {
                return this.selectedContact;
            }
            set
            {
                this.SetProperty(ref this.selectedContact, value, "SelectedContact");
            }
        }
        public int SelectedStatus
        {
            get
            {
                return this.selectedStatus;
            }
            set
            {
                this.SetProperty(ref this.selectedStatus, value, "SelectedStatus");
                if (this.netsoul.IsLogued)
                    this.netsoul.Status = (ContactStatus)this.SelectedStatus;
            }
        }

        public string Login
        {
            get
            {
                return this.login;
            }
            set
            {
                this.SetProperty(ref this.login, value, "Login");
                this.OnPropertyChanged("Image");
            }
        }
        public string Image
        {
            get
            {
                return string.Format(imagePath, this.Login);
            }
            set
            {
                this.SetProperty(ref this.login, string.Format(imagePath, value), "Image");
            }
        }

        public ICommand LoadedCMD { get; set; }
        public ICommand OpenSettingsCMD { get; set; }
        public ICommand TalkCMD { get; set; }

        public ObservableCollection<Contact> Contacts { get; set; }
        public ObservableCollection<string> Status { get; set; }

        public Home()
        {
            bool designTime = System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject());
            if (designTime == false)
            {
                this.disp = App.Current.MainWindow.Dispatcher;
                this.Contacts = new ObservableCollection<Contact>();

                this.LoadedCMD = new RelayCommand(OnLoaded);
                this.OpenSettingsCMD = new RelayCommand(() =>
                    {
                        View.Settings settings = new View.Settings();
                        settings.Show();
                    });

                this.TalkCMD = new RelayCommand(() =>
                    {
                        if (this.SelectedContact != -1)
                        {
                            View.Talk talk = new View.Talk();
                            ViewModel.Talk talkVM = new Talk(this.Login, this.Contacts[this.SelectedContact], this.netsoul);
                            talk.DataContext = talkVM;
                            talk.Show();
                        }
                    });

                this.Status = new ObservableCollection<string>();
                foreach (var st in Enum.GetValues(typeof(ContactStatus)))
                {
                    this.Status.Add(st.ToString());
                }
            }
        }

        private async void OnLoaded()
        {
            bool designTime = System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject());
            if (designTime == false)
            {
                this.netsoul = new NetsoulDesktop();
                this.netsoul.OnContactUpdate += netsoul_OnContactUpdate;

                await this.Connect();
                await this.LoadFriendList();
            }
        }

        private async Task LoadFriendList()
        {
            if (Properties.Settings.Default.Friends != null)
            {
                var lst = new List<string>();
                foreach (var friend in Properties.Settings.Default.Friends)
                {
                    await this.netsoul.AddContact(friend);
                    this.Contacts.Add(new Contact() { Login = friend, Status = ContactStatus.Offline });

                    lst.Add(friend);
                }

                // await this.netsoul.RefreshContacts(lst);
            }

            await this.netsoul.AddContact("dupova_m");
            this.Contacts.Add(new Contact() { Login = "dupova_m", Status = ContactStatus.Offline });
            await this.netsoul.AddContact("freier_n");
            this.Contacts.Add(new Contact() { Login = "freier_n", Status = ContactStatus.Offline });

            await this.netsoul.RefreshContacts(new List<string> { "freier_n", "dupova_m" });
            //await this.netsoul.RefreshContact("freier_n");
            //await this.netsoul.RefreshContact("dupova_m");
        }

        private void netsoul_OnContactUpdate(object sender, NetSoulContactUpdateEventArgs e)
        {
            var contact = this.Contacts.Where(c => c.Login == e.Contact.Login).FirstOrDefault();
            if (contact != null)
            {

                this.disp.Invoke(() =>
                   {
                       contact.Status = e.Contact.Status;
                       contact.Location = e.Contact.Location;
                       contact.UserSocket = e.Contact.UserSocket;
                   });
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
                            this.Contacts.Add(c);
                        });
                }
            }
        }

        private async Task Connect()
        {
            if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.Login) && !string.IsNullOrWhiteSpace(Properties.Settings.Default.Password))
            {
                this.netsoul.Login = Properties.Settings.Default.Login;
                this.netsoul.Password = Properties.Settings.Default.Password;
            }
            else
            {
                View.Settings settings = new View.Settings();
                settings.ShowDialog();
                this.netsoul.Login = Properties.Settings.Default.Login;
                this.netsoul.Password = Properties.Settings.Default.Password;
            }
            this.IsLogued = await this.netsoul.ConnectAsync();
            this.Login = this.netsoul.Login;
        }
    }
}
