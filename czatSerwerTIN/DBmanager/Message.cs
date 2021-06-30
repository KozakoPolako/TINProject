using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace czatSerwerTIN.DBmanager
{
    public class Message
    {

        public string sender { get; set; }
        public string timeSent { get; set; }
        public string msg { get; set; }

        public Message(string sender, string msg, long unixTime )
        {
            this.sender = sender;
            this.msg = msg;
            DateTime time = DateTimeOffset.FromUnixTimeSeconds(unixTime).LocalDateTime;
            this.timeSent = $"{time.ToShortDateString()} {time.ToShortTimeString()}";
        }

        public Message(string sender, string msg)
        {
            this.sender = sender;
            this.msg = msg;
            this.timeSent = DateTimeOffset.Now.ToUnixTimeSeconds().ToString();
        }
    }
}
