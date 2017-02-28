using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZubmarineGUI
{
    public struct InputMessage
    {
        public enum MessageType
        {
            Test
        }

        public MessageType type;
        public string data64;
    }
}
