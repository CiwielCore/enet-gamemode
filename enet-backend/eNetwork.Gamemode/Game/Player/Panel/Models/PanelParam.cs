using System;
using System.Collections.Generic;
using System.Text;

namespace eNetwork.Game.Player.Panel.Models
{
    public class PanelParam
    {
        public string k { get; set; }
        public object v { get; set; }
        public PanelParam(string key, string value)
        {
            k = key;
            v = value;
        }
        public PanelParam(string key, int value)
        {
            k = key;
            v = value;
        }
        public PanelParam(string key, long value)
        {
            k = key;
            v = value;
        }
        public PanelParam(string key, object value)
        {
            k = key;
            v = value;
        }
    }
}
