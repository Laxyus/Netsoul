using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetsoulLib.Common
{
    public static class NetsoulHelper
    {
        public static ConnectionInfo ConnectionInfoParsing(string buffer)
        {
            try
            {
                ConnectionInfo info = new ConnectionInfo();

                string[] parsing = buffer.Replace("\n", "").Split(' ');
                info.ClientSocket = Int32.Parse(parsing[1]);
                info.RandomMD5 = parsing[2];
                info.ClientIp = parsing[3];
                info.ClientPort = Int32.Parse(parsing[4]);
                info.TimeStamp = parsing[5];
                return info;
            }
            catch (Exception ex)
            {
                return null;
            }

        }

        public static string GetStatus(ContactStatus status)
        {
            switch (status)
            {
                case ContactStatus.Online:
                    return "actif";
                case ContactStatus.Away:
                    return "away";
                case ContactStatus.Connection:
                    return "connection";
                case ContactStatus.Idle:
                    return "idle";
                case ContactStatus.Lock:
                    return "lock";
                case ContactStatus.Server:
                    return "server";
                case ContactStatus.None:
                    return "none";
                default:
                    return "none";
            }
        }

        public static ContactStatus GetStatus(string status)
        {
            switch (status)
            {
                case "actif":
                    return ContactStatus.Online;
                case "away":
                    return ContactStatus.Away;
                case "connection":
                    return ContactStatus.Connection;
                case "idle":
                    return ContactStatus.Idle;
                case "lock":
                    return ContactStatus.Lock;
                case "server":
                    return ContactStatus.Server;
                case "none":
                    return ContactStatus.None;
                default:
                    return ContactStatus.None;
            }
        }
    }

    public class ConnectionInfo
    {
        public int ClientSocket { get; set; }
        public string RandomMD5 { get; set; }
        public string ClientIp { get; set; }
        public int ClientPort { get; set; }
        public string TimeStamp { get; set; }
    }
}
