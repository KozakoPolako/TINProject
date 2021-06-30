using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace czatSerwerTIN
{
    /// <summary>
    /// Klasa reprezentująca obiekt użytkownika w systemie
    /// </summary>
    public class User
    {
        /// <summary>
        /// nazwa użytkownika
        /// </summary>
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string name { get; set; }
        /// <summary>
        /// DEPRECATED
        /// </summary>
        public string isActive { get; set; }
        public User(string name, string isActive)
        {
            this.name = name;
            //this.connID = connID;
            this.isActive = isActive;
        }
    }
}
