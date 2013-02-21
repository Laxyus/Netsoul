using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Desktop.ViewModel
{
    public class Home : ViewModelBase
    {
        private bool isLogued;
        public bool IsLogued
        {
            get
            {
                return this.isLogued;
            }
            set
            {
                if (this.isLogued != value)
                {
                    this.isLogued = value;
                    this.SetProperty(ref this.isLogued, value, "IsLogued");
                }
            }
        }

        public Home()
        {

        }
    }
}
