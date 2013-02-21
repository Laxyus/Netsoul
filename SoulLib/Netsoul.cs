using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Foundation;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage;
using Windows.Storage.Streams;

namespace SoulLib
{
    public class Netsoul
    {
        private string ServerName;
        private int ServerPort;
        private string ClientSocket;
        private string RandomMD5;
        private string ClientIp;
        private string ClientPort;
        private string TimeStamp;
        private string NewMD5;

        private Task Thread;

        private List<string> ReadBufferList;
        private bool IsReading;

        private DataReader StreamReader;
        private DataWriter StreamWriter;
        private StreamSocket Client;

        /// <summary>
        /// Get the connection status of the client
        /// </summary>
        public bool IsConnected { get; private set; }

        public delegate void NetSoulErrorEventHandler(object sender, EventArgs e);
        /// <summary>
        /// Event raised when an error has occured and are documented in the Errors property.
        /// </summary>
        public event NetSoulErrorEventHandler OnErrorRaised;

        public delegate void NetSoulDataEventHandler(object sender, NetSoulDataEventArgs e);
        /// <summary>
        /// Event raised when data has been received and are ready to be used.
        /// </summary>
        public event NetSoulDataEventHandler OnDataReceived;

        public delegate void NetSoulContactUpdateEventHandler(object sender, NetSoulContactUpdateEventArgs e);
        /// <summary>
        /// Event raised when a contact information is updated by the Netsoul server.
        /// </summary>
        public event NetSoulContactUpdateEventHandler OnContactUpdated;
        /// <summary>
        /// <para>Get the list of data retrieved from NetSoul server</para>
        /// <para>Syntax: &lt;type&gt;:&lt;senderLogin&gt;:&lt;senderSocket&gt;:&lt;senderLocation&gt;:&lt;data&gt;</para>
        /// </summary>
        public List<string> Data { get; private set; }
        /// <summary>
        /// <para>Get or Set the list of message to send to a remote user</para>
        /// <para>Syntax: &lt;type&gt;:&lt;remoteLogin&gt;:&lt;message&gt;</para>
        /// <para>Example: "msg:user:message"</para>
        /// </summary>
        public List<string> Message { get; set; }
        /// <summary>
        /// User's login on the Netsoul Server
        /// </summary>
        public string UserLogin { get; set; }
        /// <summary>
        /// User's socks password
        /// </summary>
        public string UserPassword { get; set; }
        /// <summary>
        /// Location to display
        /// </summary>
        public string UserLocation { get; set; }
        /// <summary>
        /// CLient's data
        /// </summary>
        public string UserData { get; set; }
        /// <summary>
        /// Get or Set the option to shutdown the Netsoul connection
        /// </summary>
        public bool ShutdownClient { get; set; }
        /// <summary>
        /// Get or Set data to send to the server
        /// </summary>
        public List<string> SendDataList { get; set; }

        public List<string> WatchList { get; private set; }

        public List<string> Errors { get; private set; }

        public List<string> Verif;

        /// <summary>
        /// Creates a new instance of Netsoul network object.
        /// </summary>
        /// <param name="serverHostName">Represents server's IP address or domain.</param>
        /// <param name="serverPort">Represents server's listening port or service name.</param>
        public Netsoul(string serverHostName, int serverPort)
        {
            this.Verif = new List<string>();
            this.ReadBufferList = new List<string>();
            this.Client = new StreamSocket();
            this.ServerName = serverHostName;
            this.ServerPort = serverPort;
            this.Data = new List<string>();
            this.UserLogin = String.Empty;
            this.UserPassword = String.Empty;
            this.UserLocation = "none";
            this.UserData = "none";
            this.ShutdownClient = false;
            this.SendDataList = new List<string>();
            this.ClientSocket = String.Empty;
            this.RandomMD5 = String.Empty;
            this.ClientIp = String.Empty;
            this.ClientPort = String.Empty;
            this.TimeStamp = String.Empty;
            this.NewMD5 = String.Empty;
            this.WatchList = new List<string>();
            this.Message = new List<string>();
            this.Errors = new List<string>();
            this.IsReading = false;
            this.Thread = new Task(new Action(() =>
                {
                    this.NetsoulTask();
                }));
        }

        /// <summary>
        /// Initialize the connection with the specified Netsoul server.
        /// </summary>
        public async Task<bool> NetsoulConnectAsync()
        {
            if (this.UserLogin != String.Empty && this.UserPassword != String.Empty)
            {
                try
                {
                    await this.Client.ConnectAsync(new HostName(this.ServerName), this.ServerPort.ToString(), SocketProtectionLevel.PlainSocket);
                    this.IsConnected = true;
                    return true;
                }
                catch (Exception ex)
                {
                    this.ReportError(ex.Message);
                    return false;
                }
            }
            return false;
        }

        private void ReportError(string message)
        {
            this.Errors.Add(message);
            if (this.OnErrorRaised != null)
                this.OnErrorRaised(this, null);
        }

        /// <summary>
        /// Add a new person into your contact list.
        /// </summary>
        /// <param name="NewContact">Netsoul login of the new contact</param>
        public bool AddContact(string NewContact)
        {
            bool Check = false;
            foreach (string c in this.WatchList)
            {
                if (c == NewContact)
                    Check = true;
            }
            if (Check == true)
                this.ReportError("This user is already in your contacts list.");
            else
            {
                this.WatchList.Add(NewContact);
                this.SendDataList.Add(this.SendWatchList("user_cmd watch_log_user {"));
                this.SendDataList.Add(this.SendWatchList("user_cmd who {"));
                //this.SaveContactList();
            }
            return Check;
        }

        /// <summary>
        /// Add a new group of persons into your contact list.
        /// </summary>
        /// <param name="NewContact">List of Netsoul login of the new contact</param>
        public void AddContact(List<string> NewContact)
        {
            bool Check = false;
            foreach (string s in NewContact)
            {
                Check = false;
                foreach (string c in this.WatchList)
                {
                    if (c == s)
                        Check = true;
                }
                if (Check != true)
                    this.WatchList.Add(s);
            }
            this.SendDataList.Add(this.SendWatchList("user_cmd watch_log_user {") + this.SendWatchList("user_cmd who {"));
        }

        /// <summary>
        /// Remove a person from your contact list.
        /// </summary>
        /// <param name="FormerContact">Netsoul login of the contact to remove</param>
        public void RemoveContact(string FormerContact)
        {
            for (int i = 0; i < this.WatchList.Count; i++)
            {
                if (this.WatchList[i] == FormerContact)
                {
                    this.WatchList.RemoveAt(i);
                    this.SendDataList.Add(this.SendWatchList("user_cmd watch_log_user {"));
                    this.SendDataList.Add(this.SendWatchList("user_cmd who {"));
                    break;
                }
            }
        }
        /// <summary>
        /// Get a contact from the contact list.
        /// </summary>
        /// <param name="ContactLogin">Contact's login to find</param>
        /// <returns>Returns an instance of ContactInfo corresponding to the designated contact.</returns>
        public string GetContact(string ContactLogin)
        {
            string Result = null;

            IEnumerable<string> query =
                from C in this.WatchList
                where C == ContactLogin
                select C;
            if (query.ToList<string>().Count == 1)
                Result = query.ToList<string>()[0];
            return Result;
        }

        /// <summary>
        /// <para>Create the connection between this client and the designated Netsoul server.</para>
        /// <para>Send the proper identification data to the server and proceed until identification processus is complete.</para>
        /// </summary>
        /// <returns></returns>
        private async Task InitializeConnection()
        {
            this.StreamReader = new DataReader(this.Client.InputStream);
            this.StreamReader.InputStreamOptions = InputStreamOptions.Partial;
            this.StreamWriter = new DataWriter(this.Client.OutputStream);
            string buffer = String.Empty;
            uint x = await this.StreamReader.LoadAsync(75);
            buffer = this.StreamReader.ReadString(this.StreamReader.UnconsumedBufferLength);
            this.Verif.Add("<<< " + buffer);
            if (buffer.Split(' ')[0] == "salut" && buffer.Contains("\n"))
                this.ConnectionInfoParsing(buffer);
            x = await this.StreamReader.LoadAsync(74);
            buffer = this.StreamReader.ReadString(this.StreamReader.UnconsumedBufferLength);
            this.Verif.Add("<<< " + buffer);
            if (buffer == "rep 002 -- cmd end\n")
            {
                this.StreamWriter.WriteString("ext_user_log " + this.UserLogin + " " + this.NewMD5 + " " + StringToUrlConverter.Convert(this.UserLocation, ConverterMode.StandardToUrl) + " " + StringToUrlConverter.Convert(this.UserData, ConverterMode.StandardToUrl) + "\n");
                this.Verif.Add(">>> " + "ext_user_log " + this.UserLogin + " " + this.NewMD5 + " " + StringToUrlConverter.Convert(this.UserLocation, ConverterMode.StandardToUrl) + " " + StringToUrlConverter.Convert(this.UserData, ConverterMode.StandardToUrl) + "\n");
                await this.StreamWriter.StoreAsync();
            }
            x = await this.StreamReader.LoadAsync(74);
            buffer = this.StreamReader.ReadString(this.StreamReader.UnconsumedBufferLength);
            this.Verif.Add("<<< " + buffer);
            if (buffer == "rep 002 -- cmd end\n")
            {
                buffer = "user_cmd state actif\n";
                this.Verif.Add(">>> " + buffer);
                this.StreamWriter.WriteString(buffer);
                await this.StreamWriter.StoreAsync();
                buffer = String.Empty;
            }
        }
        /// <summary>
        /// Create the proper syntax for update commands who and watch_log_user 
        /// </summary>
        /// <param name="Command">Designated command</param>
        /// <returns>Return a formatted string of the designated command to be send to the Netsoul server.</returns>
        private string SendWatchList(string Command)
        {
            if (this.WatchList.Count > 0)
            {
                string buffer = Command;
                for (int i = 0; i < this.WatchList.Count; i++)
                {
                    buffer += this.WatchList[i];
                    if (i < this.WatchList.Count - 1)
                        buffer += ",";
                    else
                        buffer += "}\n";
                }
                return buffer;
            }
            return String.Empty;
        }
        /// <summary>
        /// Parse and save the user related data sent by the Netsoul server.
        /// </summary>
        /// <param name="buffer">Data to parse received from the server</param>
        private async void ConnectionInfoParsing(string buffer)
        {
            string[] parsing = buffer.Split(' ');
            this.ClientSocket = parsing[1];
            this.RandomMD5 = parsing[2];
            this.ClientIp = parsing[3];
            this.ClientPort = parsing[4];
            this.TimeStamp = parsing[5];
            HashAlgorithmProvider md5 = HashAlgorithmProvider.OpenAlgorithm("MD5");
            CryptographicHash Hasher = md5.CreateHash();
            IBuffer bufferMessage = CryptographicBuffer.ConvertStringToBinary(this.RandomMD5
                + "-" + this.ClientIp + "/" + this.ClientPort.ToString()
                + this.UserPassword, BinaryStringEncoding.Utf8);
            Hasher.Append(bufferMessage);
            IBuffer NewMD5Buffer = Hasher.GetValueAndReset();
            this.NewMD5 = CryptographicBuffer.EncodeToHexString(NewMD5Buffer);
            this.NewMD5 = this.NewMD5.ToLower();
            this.StreamWriter.WriteString("auth_ag ext_user none none\n");
            this.Verif.Add(">>> " + "auth_ag ext_user none none\n");
            await this.StreamWriter.StoreAsync();
        }
        /// <summary>
        /// Asynchronous read on the network stream.
        /// </summary>
        /// <returns>Returns the data read</returns>
        private async void ReadData()
        {
            string buffer = String.Empty;
            this.IsReading = true;
            while (buffer.Contains("\n") == false)
            {
                await this.StreamReader.LoadAsync(1024);
                buffer += this.StreamReader.ReadString(this.StreamReader.UnconsumedBufferLength);
            }
            List<string> bufferlist = buffer.Split('\n').ToList<string>();
            if (this.ReadBufferList.Count > 0 && this.ReadBufferList[this.ReadBufferList.Count - 1].Contains("\n"))
            {
                string s = this.ReadBufferList[this.ReadBufferList.Count - 1] + bufferlist[0];
                if (bufferlist.Count > 1)
                    s += "\n";
                bufferlist.RemoveAt(0);
            }
            if (bufferlist.Count > 0)
                for (int i = 0; i < bufferlist.Count; i++)
                {
                    if (i < bufferlist.Count - 1)
                        this.ReadBufferList.Add(bufferlist[i] + '\n');
                    else
                        if (bufferlist[i] != String.Empty)
                            this.ReadBufferList.Add(bufferlist[i]);
                }
            this.IsReading = false;
        }
        /// <summary>
        /// Asynchronous send on the network stream.
        /// </summary>
        private async void SendData()
        {
            await this.StreamWriter.StoreAsync();
        }

        public void StartServer()
        {
            this.Thread.Start();
        }

        /// <summary>
        /// Starting of the Network task thread.
        /// </summary>
        private async void NetsoulTask()
        {
            await this.InitializeConnection();
            await Task.Delay(100); // TEST, trouver comment enlever cette abomination!
            while (this.ShutdownClient == false)
            {
                while (this.SendDataList.Count > 0)
                {
                    string str = this.SendDataList[0];
                    if (str != null)
                    {
                        this.Verif.Add(">>> " + str);
                        this.StreamWriter.WriteString(str);
                        this.SendData();
                    }
                    this.SendDataList.RemoveAt(0);
                }
                if (this.IsReading == false)
                    this.ReadData();
                if (this.ReadBufferList.Count > 0)
                    this.ParseReceivedData(this.ReadBufferList[0]);
                if (this.Message.Count > 0)
                    this.FormatData();
            }
        }
        /// <summary>
        /// Format the data to send into Netsoul Protocol syntax
        /// </summary>
        private void FormatData()
        {

            string formatted = String.Empty;
            formatted = "user_cmd msg_user {:" + this.Message[0].Split(':')[0] + "} msg " + StringToUrlConverter.Convert(this.Message[0].Split(':')[1] + "\n", ConverterMode.StandardToUrl);
            this.StreamWriter.WriteString(formatted);
            this.Message.RemoveAt(0);
            this.Verif.Add(">>> " + formatted);
            this.SendData();
        }

        /// <summary>
        /// Parse the data received from the Netsoul server.
        /// </summary>
        /// <param name="buffer">Data received</param>
        private void ParseReceivedData(string buffer)
        {
            this.Verif.Add("<<< " + this.ReadBufferList[0]);
            if (buffer != null)
            {
                this.Verif.Add("<<<" + buffer);
                string[] command = buffer.Split(' ');
                switch (command[0])
                {
                    case "ping":
                        this.SendDataList.Add(buffer);
                        break;
                    case "user_cmd":
                        if (buffer.Contains("|"))
                        {
                            string[] separator = new string[1];
                            separator[0] = " ";
                            string[] userCommand = buffer.Split('|')[1].Split(separator, StringSplitOptions.RemoveEmptyEntries);
                            string[] parse = null;
                            switch (userCommand[0])
                            {
                                case "msg":
                                    parse = buffer.Split('|')[1].Split(' ');
                                    this.Data.Add("message:" + buffer.Split('|')[0].Split(':')[3].Split('@')[0] + ":"
                                                            + buffer.Split('|')[0].Split(':')[0].Split(' ')[1] + ":"
                                                            + buffer.Split('|')[0].Split(':')[5] + ":"
                                                            + StringToUrlConverter.Convert(parse[2], ConverterMode.UrlToStandard));
                                    this.RaiseNetSoulDataEvent(buffer);
                                    break;
                                case "who":
                                    parse = buffer.Split('|')[1].Split(' ');
                                    if (parse[2] == "rep")
                                        break;
                                    this.UpdateContact(parse[3], parse[12].Split(':')[0], parse[10], parse[13].Split('\n')[0], parse[2]);
                                    break;
                                case "login":
                                    parse = command[1].Split(':');
                                    this.SendDataList.Add("user_cmd who {" + parse[3].Split('@')[0] + "}\n");
                                    break;
                                case "state":
                                    parse = command[1].Split(':');
                                    this.LogUpdate(userCommand[0], parse[3].Split('@')[0], parse[0], userCommand[1].Split(':')[0]);
                                    break;
                                case "logout":
                                    parse = command[1].Split(':');
                                    this.LogUpdate(userCommand[0], parse[3].Split('@')[0], parse[0], null);
                                    break;
                                default:
                                    break;
                            }
                        }
                        break;
                    default:
                        break;
                }
                this.ReadBufferList.RemoveAt(0);
            }
        }
        /// <summary>
        /// Create the NetSoulContactUpdateEventArgs and raises OnContactUpdated when a change of state occurs
        /// </summary>
        /// <param name="Event">Type of change of state</param>
        /// <param name="login">Login of the concerned contact</param>
        /// <param name="socket">Socket of the concerned contact</param>
        /// <param name="data">New state of the concerned contact (in case of logout event, data is null)</param>
        private void LogUpdate(string Event, string login, string socket, string data)
        {
            NetSoulContactUpdateEventArgs e = null;
            switch (Event)
            {
                case "logout":
                    e = new NetSoulContactUpdateEventArgs("logout:" + login + ":" + socket);
                    break;
                case "state":
                    e = new NetSoulContactUpdateEventArgs("state:" + login + ":" + socket + ":" + data);
                    break;
                default:
                    break;
            }
            if (this.OnContactUpdated != null && e != null)
                this.OnContactUpdated(this, e);
        }
        /// <summary>
        /// Create the NetSoulContactUpdateEventArgs and raises OnContactUpdated when a WHO command is received
        /// </summary>
        /// <param name="login">Login of the concerned contact</param>
        /// <param name="status">State of the concerned contact</param>
        /// <param name="location">Location of the concerned contact</param>
        /// <param name="userdata">UserData of the concerned contact</param>
        /// <param name="socket">Socket of the concerned contact</param>
        private void UpdateContact(string login, string status, string location, string userdata, string socket)
        {
            NetSoulContactUpdateEventArgs e = new NetSoulContactUpdateEventArgs("update:" + login + ":" + status + ":" + location + ":" + userdata + ":" + socket);
            if (this.OnContactUpdated != null)
                this.OnContactUpdated(this, e);
        }
        /// <summary>
        /// Create the NetSoulDataEventArgs and raise OnDataReceived event 
        /// </summary>
        /// <param name="data"></param>
        private void RaiseNetSoulDataEvent(string data)
        {
            NetSoulDataEventArgs e = new NetSoulDataEventArgs(data);
            if (e != null && this.OnDataReceived != null)
                this.OnDataReceived(this, e);
        }
    }
}
