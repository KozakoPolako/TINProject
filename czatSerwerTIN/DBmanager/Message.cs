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
        public string type { get; set; }

        public Message(string sender, string msg, string type, long unixTime )
        {
            this.sender = sender;
            this.msg = msg;
            DateTime time = DateTimeOffset.FromUnixTimeSeconds(unixTime).LocalDateTime;
            this.timeSent = $"{time.ToShortDateString()} {time.ToShortTimeString()}";
            this.type = type;
        }

        public Message(string sender, string msg, string type)
        {
            this.sender = sender;
            this.msg = msg;
            this.timeSent = DateTimeOffset.Now.ToUnixTimeSeconds().ToString();
            this.type = type;
        }

        public void convertTimeSentToDateFormat()
        {
            DateTime time = DateTimeOffset.FromUnixTimeSeconds(long.Parse(timeSent)).LocalDateTime;
            this.timeSent = $"{time.ToShortDateString()} {time.ToShortTimeString()}";
        }
    }
}
