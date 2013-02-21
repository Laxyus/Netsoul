using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulLib
{
    public enum ConverterMode
    {
        UrlToStandard,
        StandardToUrl
    }

    public class StringToUrlConverter
    {
        public static string Convert(string toConvert, ConverterMode mode)
        {
            Dictionary<string, string> CharacterSet = new Dictionary<string, string>()
            {
                { "?", "%3f" },
                { ">", "%3e" },
                { "<", "%3c" },
                { "|", "%7c" },
                { "\\", "%5c" },
                { "]", "%5d" },
                { "[", "%5b" },
                { "}", "%7d" },
                { "{", "%7b" },
                { "+", "%2b" },
                { "=", "%3d" },
                { ")", "%29" },
                { "(", "%28" },
                { "*", "%2a" },
                { "&", "%26" },
                { "^", "%5e" },
                { "%", "%25" },
                { "$", "%24" },
                { "#", "%23" },
                { "@", "%40" },
                { "!", "%21" },
                { "~", "%7e" },
                { "`", "%60" },
                { ",", "%82" },
                { "/", "%2f" },
                { ":", "%3a" },
                { ";", "%3b" },
                { "€", "%80" },
                { " ", "%20" }
            };

            string Converted = toConvert;

            if (mode == ConverterMode.StandardToUrl)
                foreach (KeyValuePair<string, string> s in CharacterSet)
                    Converted = Converted.Replace(s.Key, s.Value);
            else
                foreach (KeyValuePair<string, string> s in CharacterSet)
                    Converted = Converted.Replace(s.Value, s.Key);
            return Converted;
        }
    }
}
