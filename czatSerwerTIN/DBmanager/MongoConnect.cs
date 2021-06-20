using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace czatSerwerTIN.DBmanager
{


    public class MongoConnect
    {
        string CONNECTION_STRING = "mongodb://darekddd:Password1!@localhost:2717";
        //string CONNECTION_STRING = "mongodb://localhost:2717";
        MongoClient client;
        IMongoDatabase database;
        IMongoCollection<BsonDocument> users;

        public MongoConnect()
        {
            client = new MongoClient(CONNECTION_STRING);
            database = client.GetDatabase("CzatDB");
            users = database.GetCollection<BsonDocument>("users");
        }

        // dodaje nowego użytkownika tylko w sytuacji kiedy nie istnieje dokument o kluczu Name = name 
        public Task InsertUser(string name)
        {
           /* var document = new BsonDocument
            {
                { "Name",name},
                {BulkWriteUpsert. }
            };*/

            var filter = Builders<BsonDocument>.Filter.Eq("Name", name);
            var update = Builders<BsonDocument>.Update.Set("Name",name);
            var options = new UpdateOptions { IsUpsert = true };
            return users.UpdateOneAsync(filter, update, options);
        }
        public List<string> GetUserList()
        {
            List<string> Users = new List<string>();
            
            users.Find(_ => true).ToList().ForEach(element => {
                Users.Add(element["Name"].AsString);
            });
            return Users;
                
        }
        
    }
}
