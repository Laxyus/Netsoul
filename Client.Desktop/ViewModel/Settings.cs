using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Desktop.Helpers;

namespace Desktop.ViewModel
{
    public class Settings : ViewModelBase
    {
        private string login;
        private string password;

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

        public string Password
        {
            get
            {
                return this.password;
            }
            set
            {
                this.SetProperty(ref this.password, value, "Password");
            }
        }

        public ICommand SaveCMD { get; set; }

        public Settings()
        {
            this.SaveCMD = new RelayCommand(this.Save);
            this.Login = Properties.Settings.Default.Login;
            this.Password = Properties.Settings.Default.Password;
        }

        private void Save()
        {
            Properties.Settings.Default.Login = this.Login;
            Properties.Settings.Default.Password = this.Password;
            Properties.Settings.Default.Save();
        }

    }
}
