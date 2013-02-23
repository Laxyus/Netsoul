using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetsoulLib.Common
{
    public abstract class Netsoul : INetsoul
    {
        private ContactStatus status;

        public string Server { get; set; }
        public int ServerPort { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string UserLocation { get; set; }
        public string UserData { get; set; }

        public ContactStatus Status
        {
            get
            {
                return this.status;
            }
            set
            {
                if (this.IsLogued)
                {
                    SetStatus(value);
                }
                this.status = value;
            }
        }

        public bool IsLogued { get; private set; }

        public event EventHandler<NetsoulMessageEventArgs> OnMessage;
        public event EventHandler<NetSoulContactUpdateEventArgs> OnContactUpdate;

        public Netsoul()
        {
            this.UserLocation = "none";
            this.UserData = "none";
            this.Server = "ns-server.epita.fr";
            this.ServerPort = 4242;
            this.IsLogued = false;
        }

        public async Task<bool> ConnectAsync()
        {
            if (this.Login != String.Empty && this.Password != String.Empty)
            {
                try
                {
                    if (await this.Init())
                    {
                        var info = await InitConnection();
                        if (info != null)
                        {
                            var md5 = this.CalcMD5(info.RandomMD5 + "-" + info.ClientIp + "/" + info.ClientPort.ToString() + this.Password);
                            var ret = await this.SendData(string.Format("auth_ag ext_user {0} {1}\n", "none", this.UserLocation));
                            if (ret == true)
                            {
                                var buffer = await this.ReadData();
                                if (buffer == "rep 002 -- cmd end\n")
                                {
                                    ret = await this.SendData("ext_user_log " + this.Login + " " + md5 + " " + StringToUrlConverter.Convert(this.UserLocation, ConverterMode.StandardToUrl) + " " + StringToUrlConverter.Convert(this.UserData, ConverterMode.StandardToUrl) + "\n");
                                    if (ret == true)
                                    {
                                        buffer = await this.ReadData();
                                        if (buffer == "rep 002 -- cmd end\n")
                                        {
                                            ret = await SetStatus(ContactStatus.Online);
                                            this.InitListener();
                                            this.IsLogued = true;
                                            return this.IsLogued;
                                        }
                                    }
                                }
                            }

                        }
                    }
                }
                catch (Exception ex)
                {
                    return false;
                }
            }

            return false;
        }

        private async Task<bool> SetStatus(ContactStatus status)
        {
            return await this.SendData(string.Format("user_cmd state {0}\n", NetsoulHelper.GetStatus(status)));
        }

        public async Task<bool> AddContact(string contact)
        {
            return await this.AddContacts(new List<string>() { contact });
        }

        public async Task<bool> AddContacts(List<string> contacts)
        {
            StringBuilder str = new StringBuilder("user_cmd watch_log_user {");
            for (int i = 0; i < contacts.Count; i++)
            {
                if (i > 0)
                    str.Append(",");
                str.Append(contacts[i]);
            }
            str.Append("}\n");

            return await this.SendData(str.ToString());
        }

        protected async Task ParseReceivedData(string buffer)
        {
            if (buffer != null)
            {
                string[] command = buffer.Split(' ');
                switch (command[0])
                {
                    case "ping":
                        await this.SendData(buffer);
                        break;
                    case "user_cmd":
                        if (buffer.Contains("|"))
                        {
                            string[] userCommand = buffer.Split('|')[1].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                            string[] parse = null;
                            switch (userCommand[0])
                            {
                                case "msg":
                                    parse = buffer.Split('|')[1].Split(' ');
                                    var msg = "message:" + buffer.Split('|')[0].Split(':')[3].Split('@')[0] + ":"
                                                            + buffer.Split('|')[0].Split(':')[0].Split(' ')[1] + ":"
                                                            + buffer.Split('|')[0].Split(':')[5] + ":"
                                                            + StringToUrlConverter.Convert(parse[2], ConverterMode.UrlToStandard);
                                    if (this.OnMessage != null)
                                        this.OnMessage(this, new NetsoulMessageEventArgs(msg));
                                    break;
                                case "who":
                                    parse = buffer.Split('|')[1].Split(' ');
                                    if (parse[2] == "rep")
                                        break;
                                    //this.UpdateContact(parse[3], parse[12].Split(':')[0], parse[10], parse[13].Split('\n')[0], parse[2]);
                                    break;
                                case "login":
                                case "state":
                                    parse = command[1].Split(':');
                                    var status = userCommand[1].Split(':')[0];
                                    if (this.OnContactUpdate != null)
                                        this.OnContactUpdate(this, new NetSoulContactUpdateEventArgs(parse[3].Split('@')[0], NetsoulHelper.GetStatus(status), parse[5], "", parse[0]));
                                    break;
                                case "logout":
                                    parse = command[1].Split(':');
                                    if (this.OnContactUpdate != null)
                                        this.OnContactUpdate(this, new NetSoulContactUpdateEventArgs(parse[3].Split('@')[0], ContactStatus.Offline, null, null, "-1"));
                                    break;
                                default:
                                    break;
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private async Task<ConnectionInfo> InitConnection()
        {
            var buffer = await this.ReadData();
            if (buffer.Split(' ')[0] == "salut" && buffer.Contains("\n"))
            {
                var info = NetsoulHelper.ConnectionInfoParsing(buffer);
                return info;

            }
            return null;
        }

        public async virtual Task<bool> Init()
        {
            return false;
        }
        public async virtual Task<string> ReadData()
        {
            return null;
        }

        public virtual string CalcMD5(string str)
        {
            return null;
        }

        public async virtual Task<bool> SendData(string str)
        {
            return false;
        }

        private bool InitListener()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    var buffer = this.ReadData();
                    buffer.Wait();
                    this.ParseReceivedData(buffer.Result);
                    Debug.WriteLine(buffer);
                }
            });

            return true;
        }

        public Task<bool> Send(int userSocket, string message)
        {
            var formatted = "user_cmd msg_user {:" + userSocket + "} msg " + StringToUrlConverter.Convert(message + "\n", ConverterMode.StandardToUrl);
            return this.SendData(formatted);
        }
    }
}
