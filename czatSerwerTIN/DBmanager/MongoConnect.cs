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
        public Task InsertUser(string name, string connID)
        {
           /* var document = new BsonDocument
            {
                { "Name",name},
                {BulkWriteUpsert. }
            };*/

                
            var filter = Builders<BsonDocument>.Filter.Eq("Name", name);
            var update = Builders<BsonDocument>.Update.Set("Name",name ) 
                .Set("ConnID",connID)
                .Set("IsActive", "true");
            var options = new UpdateOptions { IsUpsert = true };
            return users.UpdateOneAsync(filter, update, options);
        }
        public Task LogoutUser( string connID)
        {
           

            var filter = Builders<BsonDocument>.Filter.Eq("ConnID", connID);
            var update = Builders<BsonDocument>.Update.Set("IsActive", "false");
                
            return users.UpdateOneAsync(filter, update);
        }

        public Task<IAsyncCursor<BsonDocument>> GetActiveUsers()
        {

            var filter = Builders<BsonDocument>.Filter.Eq("IsActive", "true");
            //string Users = users.FindAsync(filter).;


            return users.FindAsync(filter); 
                
        }
        
    }
}
