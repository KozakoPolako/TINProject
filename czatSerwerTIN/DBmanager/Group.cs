using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace czatSerwerTIN.DBmanager
{
    public class Group
    {
        public string groupName { get; }
        public string groupType { get; }
        public List<Message> content { get; set; }

        public Group(string groupName, string groupType)
        {
            this.groupName = groupName;
            this.groupType = groupType;
            content = new();
        }

        public void addMessage(string sender, string msg)
        {
            content.Add(new Message(sender, msg));
        }

        public void addMessage(Message msg)
        {
            content.Add(msg);
        }

        public void addMultipleMessages(List<Message> messages)
        {
            content.AddRange(messages);
        }

    }
}
