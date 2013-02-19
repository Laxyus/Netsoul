using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace Windows8
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class LoginPage : Windows8.Common.LayoutAwarePage
    {
        ConnectionInfo Connection;

        public LoginPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Required;
            this.Loaded += LoginPage_Loaded;
        }

        void LoginPage_Loaded(object sender, RoutedEventArgs e)
        {
            this.OpenFile();
        }

        async void OpenFile()
        {
            try
            {
                StorageFolder appData = await ApplicationData.Current.RoamingFolder.CreateFolderAsync("MetroSoul", CreationCollisionOption.OpenIfExists);
                StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(appData.Path);
                try
                {
                    StorageFile file = await folder.GetFileAsync("remember.xml");
                    try
                    {
                        using (Stream Str = await file.OpenStreamForReadAsync())
                        {
                            XmlSerializer xs = new XmlSerializer(typeof(ConnectionInfo));
                            this.Connection = (ConnectionInfo)xs.Deserialize(Str);
                            this.tbServer.Text = this.Connection.Server;
                            this.tbPort.Text = this.Connection.Port;
                            this.tbLogin.Text = this.Connection.Login;
                            this.tbPassword.Password = this.Connection.Password;
                            this.tbLocation.Text = this.Connection.Location;
                            this.cbServer.IsChecked = this.Connection.RemServer;
                            this.cbPort.IsChecked = this.Connection.RemPort;
                            this.cbLogin.IsChecked = this.Connection.RemLogin;
                            this.cbPassword.IsChecked = this.Connection.RemPassword;
                            this.cbLocation.IsChecked = this.Connection.RemLocation;
                            this.TextBox_KeyDown_1(null, null);
                        }
                    }
                    catch
                    {
                        NetsoulNotificationSystem.DisplayNotification("Impossible to load your connection data", NetsoulNotificationType.Error);
                    }
                }
                catch
                {
                }
            }
            catch
            {
                NetsoulNotificationSystem.DisplayNotification("Impossible to create and/or open MetroSoul folder", NetsoulNotificationType.Error);
            }
        }

        private async Task<StorageFolder> CreateMetroSoulDirectory()
        {
            StorageFolder sf = null;
            try
            {
                sf = await ApplicationData.Current.RoamingFolder.CreateFolderAsync("MetroSoul", CreationCollisionOption.FailIfExists);
            }
            catch
            {
                NetsoulNotificationSystem.DisplayNotification("Creation of MetroSoul directory failed", NetsoulNotificationType.Error);
            }
            return sf;
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
        }

        private void TextBox_KeyDown_1(object sender, KeyRoutedEventArgs e)
        {
            if (this.tbServer.Text != String.Empty && this.tbPort.Text != String.Empty && this.tbLogin.Text != String.Empty && this.tbPassword.Password != String.Empty)
                this.btnConnect.IsEnabled = true;
            else
                this.btnConnect.IsEnabled = false;
        }

        private async void btnConnect_Click_1(object sender, RoutedEventArgs e)
        {
            if (this.Frame.CanGoForward == false)
            {
                string[] parameters = new string[5];
                parameters[0] = this.tbServer.Text;
                parameters[1] = this.tbPort.Text;
                parameters[2] = this.tbLogin.Text;
                parameters[3] = this.tbPassword.Password;
                parameters[4] = this.tbLocation.Text;
                if (this.Connection == null)
                    this.Connection = new ConnectionInfo();
                if (this.cbServer.IsChecked == true)
                    this.Connection.Server = this.tbServer.Text;
                else
                    this.Connection.Server = String.Empty;
                if (this.cbPort.IsChecked == true)
                    this.Connection.Port = this.tbPort.Text;
                else
                    this.Connection.Port = String.Empty;
                if (this.cbLogin.IsChecked == true)
                    this.Connection.Login = this.tbLogin.Text;
                else
                    this.Connection.Login = String.Empty;
                if (this.cbPassword.IsChecked == true)
                    this.Connection.Password = this.tbPassword.Password;
                else
                    this.Connection.Password = String.Empty;
                if (this.cbLocation.IsChecked == true)
                    this.Connection.Location = this.tbLocation.Text;
                else
                    this.Connection.Location = String.Empty;
                this.Connection.RemServer = this.cbServer.IsChecked.Value;
                this.Connection.RemPort = this.cbPort.IsChecked.Value;
                this.Connection.RemLogin = this.cbLogin.IsChecked.Value;
                this.Connection.RemPassword = this.cbPassword.IsChecked.Value;
                this.Connection.RemLocation = this.cbLocation.IsChecked.Value;
                this.tbServer.IsEnabled = false;
                this.tbPort.IsEnabled = false;
                this.tbLogin.IsEnabled = false;
                this.tbPassword.IsEnabled = false;
                this.tbLocation.IsEnabled = false;
                await this.SaveParameters();
                this.Frame.Navigate(typeof(NetsoulPage), parameters);
            }
            else
                this.Frame.GoForward();
        }

        private async Task SaveParameters()
        {
            StorageFolder sf = null;

            try
            {
                sf = await ApplicationData.Current.RoamingFolder.GetFolderAsync("MetroSoul");
            }
            catch
            {
                sf = this.CreateMetroSoulDirectory().Result;
            }
            if (sf != null)
            {
                StorageFile file = await sf.CreateFileAsync("remember.xml", CreationCollisionOption.ReplaceExisting);
                using (Stream fileStream = await file.OpenStreamForWriteAsync())
                {
                    XmlSerializer xs = new XmlSerializer(typeof(ConnectionInfo));
                    xs.Serialize(fileStream, this.Connection);
                }
            }
            else
            {
                NetsoulNotificationSystem.DisplayNotification("Saving connection data failed", NetsoulNotificationType.Error);
            }
        }
    }
}
