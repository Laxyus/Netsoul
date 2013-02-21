using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SoulLib
{
    public class NetSoulDataEventArgs : EventArgs
    {
        public string UnformattedData { get; set; }

        public NetSoulDataEventArgs(string data)
        {
            this.UnformattedData = data;
        }
    }
}
