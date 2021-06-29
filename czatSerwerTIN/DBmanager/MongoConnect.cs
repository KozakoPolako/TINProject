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
        IMongoCollection<BsonDocument> groups;

        public MongoConnect()
        {
            client = new MongoClient(CONNECTION_STRING);
            database = client.GetDatabase("CzatDB");
            users = database.GetCollection<BsonDocument>("users");
            groups = database.GetCollection<BsonDocument>("groups");
        }

        // dodaje nowego użytkownika tylko w sytuacji kiedy nie istnieje dokument o kluczu Name = name 
        public async Task<bool> InsertUser(string name, string password)
        {
            bool status = false;
            int usrCount = 0;

            var cursor = await users.FindAsync("{Name: {$eq: \""+name+"\"}}");
            await cursor.ForEachAsync(user => {
                if (user["Password"].AsString == password) status = true;
                usrCount++;
            });
            // konto nie istnieje 
            if (usrCount == 0)
            {
                var filter = Builders<BsonDocument>.Filter.Eq("Name", name);
                var update = Builders<BsonDocument>.Update.Set("Password", password)
                    .Set("IsActive", "true");
                var options = new UpdateOptions { IsUpsert = true };
                await users.UpdateOneAsync(filter, update, options);
                status = true;
            }else
            // podano prawidłowe dane logowania 
            if (usrCount == 1 && status)
            {
                var filter = Builders<BsonDocument>.Filter.Eq("Name", name);
                var update = Builders<BsonDocument>.Update.Set("IsActive", "true");
                var options = new UpdateOptions { IsUpsert = true };
                await users.UpdateOneAsync(filter, update, options);
            }else { status = false; }
            
            return status;
        }

        public Task AddUserToGroup(string name, string groupName)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("GroupName", groupName);
            var update = Builders<BsonDocument>.Update.AddToSet("Members", name);

            var options = new UpdateOptions { IsUpsert = true };

            return groups.UpdateOneAsync(filter, update, options);
        }

        public Task RemoveUserFromGroup(string name, string groupName)
        {
            return groups.UpdateOneAsync("{GroupName: \"" + groupName + "\"}","{$pull: {Members: \""+name+"\"}}");
        }
        public Task LogoutUser(string username)
        {


            var filter = Builders<BsonDocument>.Filter.Eq("Name", username);
            var update = Builders<BsonDocument>.Update.Set("IsActive", "false");

            return users.UpdateOneAsync(filter, update);
        }
        public Task<IAsyncCursor<BsonDocument>> GetGroupUsers(string groupname)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("GroupName", groupname);
            return groups.FindAsync(filter);
        }
        public async Task SaveMessageGroupMessage(string username, string message, string groupname)
        {
            DateTime foo = DateTime.Now;
            long unixTime = ((DateTimeOffset)foo).ToUnixTimeSeconds();
            await groups.UpdateOneAsync("{ GroupName:\"" + groupname + "\" }", "{ $addToSet: { Content: { Sender: \"" + username + "\", Time: \"" + unixTime.ToString() + "\", Message: \"" + message + "\"} } }");
        }
        /// <summary>
        /// Wczytuje wszystkie wiadomosci z danego groupname z bazy danych. Zwrotka jest w postaci <c>&lt;IAsyncCursor&lt;BsonDocument&gt;&gt;</c>
        /// </summary>
        /// <param name="groupname"></param>
        /// <returns></returns>
        public Task<IAsyncCursor<BsonDocument>> LoadMessagesByGroupName(string groupname)
        {
            return groups.FindAsync("{GroupName: \"" + groupname + "\"}, {Content:1, _id:0}");
        }
        public async Task SavePrivateMessage(string username, string message, string groupname)
        {
            DateTime foo = DateTime.Now;
            long unixTime = ((DateTimeOffset)foo).ToUnixTimeSeconds();
            await groups.UpdateOneAsync("{ GroupName:" + groupname + " }", "{ $addToSet: { Content: { Sender: " + username + ", Time: " + unixTime.ToString() + ", Message: " + message + "} } }", "{upsert: true}");
        }


        public Task<IAsyncCursor<BsonDocument>> GetUsers()
        {

            //var filter = Builders<BsonDocument>.Filter.Eq("IsActive", "true");
            //string Users = users.FindAsync(filter).;


            return users.FindAsync(_ => true);

        }
        public Task<IAsyncCursor<BsonDocument>> GetGroups(string user)
        {

            //{Members:{$elemMatch: {$eq:"Darek"}}}
            //var filter = new BsonDocument {{$elemMatch: {$eq: "Darek"} } };

    //var filter = Builders<BsonDocument>.Filter.ElemMatch(x => x["Members"], user);


    //var filter = Builders<>.Filter.ElemMatch( x => x.Name == "test");
    //var options = new FindOptions {  }; 
    //Console.WriteLine("jojojjojo");

            return groups.FindAsync("{Members:{$elemMatch: {$eq:\""+user+"\"}}}");

        }
        public Task LogoutAll()
        {
            var filter = Builders<BsonDocument>.Filter.Eq("IsActive", "true");
            var update = Builders<BsonDocument>.Update.Set("IsActive", "false");
                
            return users.UpdateManyAsync(filter, update);
        }

    }
}
