using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using Desktop.Helpers;
using NetsoulLib.Common;

namespace Desktop.ViewModel
{
    public class Talk : ViewModelBase
    {
        private const string imagePath = "https://www.epitech.eu/intra/photos/{0}.jpg";

        private Dispatcher disp;
        private string loginSender;
        private string imageSender;
        private string login;
        private string image;
        private Contact contact;
        private INetsoul netsoul;
        private string text;

        public string LoginSender
        {
            get
            {
                return this.loginSender;
            }
            set
            {
                this.SetProperty(ref this.loginSender, value, "LoginSender");
                this.OnPropertyChanged("ImageSender");
            }
        }
        public string ImageSender
        {
            get
            {
                return string.Format(imageSender, this.Login);
            }
            set
            {
                this.SetProperty(ref this.imageSender, string.Format(imagePath, value), "ImageSender");
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
                return string.Format(image, this.Login);
            }
            set
            {
                this.SetProperty(ref this.image, string.Format(imagePath, value), "Image");
            }
        }

        public string Text
        {
            get
            {
                return this.text;
            }
            set
            {
                this.SetProperty(ref this.text, value, "Text");
            }
        }

        public ObservableCollection<string> Messages { get; set; }

        public ICommand SendCMD { get; set; }

        public Talk(string login, Contact contact, INetsoul netsoul)
        {
            this.disp = App.Current.MainWindow.Dispatcher;
            this.SendCMD = new RelayCommand(this.Send);

            this.netsoul = netsoul;

            this.Messages = new ObservableCollection<string>();
            this.contact = contact;
            this.Login = login;
            this.LoginSender = contact.Login;

            netsoul.OnMessage += netsoul_OnMessage;
        }

        private void Send()
        {
            this.netsoul.Send(this.contact.UserSocket, this.Text);
            this.InsertMessage(this.login, this.Text);
            this.text = string.Empty;
        }

        void netsoul_OnMessage(object sender, NetsoulMessageEventArgs e)
        {
            if (e.Login != this.LoginSender)
                return;

            this.disp.Invoke(() =>
                       {
                           this.InsertMessage(this.LoginSender, e.Message);
                       });
        }

        private void InsertMessage(string from, string message)
        {
            this.Messages.Insert(this.Messages.Count, string.Format("{0} => {1}", from, message));
        }
    }
}
