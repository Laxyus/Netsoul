using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetsoulLib.Common
{
    public class NetsoulMessageEventArgs : EventArgs
    {
        public string Login { get; set; }
        public string Message { get; set; }
        public string Location { get; set; }
        public int UserSocket { get; set; }

        public NetsoulMessageEventArgs(string data)
        {
            var d = data.Split(':');

            if (d.Count() >= 5)
            {
                this.UserSocket = int.Parse(d[2]);
                this.Login = d[1];
                this.Message = d[4];
                this.Location = d[3];
            }
        }
    }
}
