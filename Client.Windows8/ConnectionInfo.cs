using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Windows8
{
    public class ConnectionInfo
    {
        public string Login { get; set; }
        public string Password { get; set; }
        public string Server { get; set; }
        public string Port { get; set; }
        public string Location { get; set; }
        public bool RemLogin { get; set; }
        public bool RemPassword { get; set; }
        public bool RemServer { get; set; }
        public bool RemPort { get; set; }
        public bool RemLocation { get; set; }

        public ConnectionInfo(string srv, string prt, string log, string pwd, string loc)
        {
            this.Server = srv;
            this.Port = prt;
            this.Login = log;
            this.Password = pwd;
            this.Location = loc;
            this.RemServer = false;
            this.RemPort = false;
            this.RemLogin = false;
            this.RemPassword = false;
            this.RemLocation = false;
        }

        public ConnectionInfo()
        {
            this.Login = String.Empty;
            this.Password = String.Empty;
            this.Server = String.Empty;
            this.Port = String.Empty;
            this.Location = String.Empty;
            this.RemServer = false;
            this.RemPort = false;
            this.RemLogin = false;
            this.RemPassword = false;
            this.RemLocation = false;
        }
    }
}
