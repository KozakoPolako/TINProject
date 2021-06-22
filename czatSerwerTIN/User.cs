using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace czatSerwerTIN
{
    public class User
    {
       
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        
        public string connID { get; set; }
        public string name { get; set; }
        public Boolean isActive { get; set; }
        public User(string name, string connID, bool isActive)
        {
            this.name = name;
            this.connID = connID;
            this.isActive = isActive;
        }
    }
}
