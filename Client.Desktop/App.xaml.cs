using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using NetsoulLib.Common;
using NetsoulLib.Desktop;

namespace Desktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        INetsoul netsoul = new NetsoulDesktop();

        public App()
        {
            test();
        }

        public async void test()
        {
            netsoul.Login = "freier_n";
            netsoul.Password = "a3L[aDn5";
            netsoul.OnMessage += netsoul_OnMessage;
            var ret = await netsoul.ConnectAsync();
        }

        void netsoul_OnMessage(object sender, NetsoulMessageEventArgs e)
        {
            netsoul.Send(e.UserSocket, "kikou");
        }
    }
}
