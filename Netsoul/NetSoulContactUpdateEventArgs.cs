using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetsoulLib
{
    public class NetSoulContactUpdateEventArgs : EventArgs
    {
        public string UpdatedContact { get; set; }

        public NetSoulContactUpdateEventArgs(string Update)
        {
            this.UpdatedContact = Update;
        }
    }
}
