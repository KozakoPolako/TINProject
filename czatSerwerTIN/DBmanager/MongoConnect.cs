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
            if (usrCount > 1 )
            {
                status = false;
                //var filter = Builders<BsonDocument>.Filter.Eq("Name", name);
                //var update = Builders<BsonDocument>.Update.Set("IsActive", "true");
                //var options = new UpdateOptions { IsUpsert = true };
                //await users.UpdateOneAsync(filter, update, options);
            }
            
            return status;
        }

        public Task AddUserToGroup(string name, string groupName)
        {
            //var filter = Builders<BsonDocument>.Filter.Eq("GroupName", groupName);
            var update = Builders<BsonDocument>.Update.AddToSet("Members", name);

            var options = new UpdateOptions { IsUpsert = true };

            return groups.UpdateOneAsync("{GroupName: \""+groupName+ "\", Type: \"Public\"}", update, options);
        }

        public Task RemoveUserFromGroup(string name, string groupName)
        {
            return groups.UpdateOneAsync("{GroupName: \"" + groupName + "\", Type: \"Public\"}","{$pull: {Members: \""+name+"\"}}");
        }
        //public Task LogoutUser(string username)
        //{
        //    var filter = Builders<BsonDocument>.Filter.Eq("Name", username);
        //    var update = Builders<BsonDocument>.Update.Set("IsActive", "false");

        //    return users.UpdateOneAsync(filter, update);
        //}
        //public Task<IAsyncCursor<BsonDocument>> GetGroupUsers(string groupname)
        //{
        //    var filter = Builders<BsonDocument>.Filter.Eq("GroupName", groupname);
        //    return groups.FindAsync(filter);
        //}
        public async Task SaveGroupMessage(Message message, string groupname)
        {
            await groups.UpdateOneAsync("{ GroupName:\"" + groupname + "\", Type: \"Public\" }", "{ $addToSet: { Content: { Sender: \"" + message.sender + "\", Time: " + message.timeSent + ", Message: \"" + message.msg + "\"} } }");
        }
        /// <summary>
        /// Wczytuje wszystkie wiadomosci z danego groupname z bazy danych. Zwrotka jest w postaci <c>&lt;IAsyncCursor&lt;BsonDocument&gt;&gt;</c>
        /// </summary>
        /// <param name="groupname"></param>
        /// <returns></returns>
        public async Task<Group> LoadMessagesByGroupName(string groupName, string groupType)
        {
            Group group = new Group(groupName, groupType);
            string filter = "{GroupName: \"" + groupName + "\", Type: \"" + groupType + "\"}";
            Console.WriteLine(filter);
            var options = new FindOptions <BsonDocument>() { 
                Projection =Builders<BsonDocument>.Projection
                    .Include("Content")
                    .Exclude("_id")
            };
            var cursor = await groups.FindAsync(filter,options);
            if(cursor.Current != null)
            {
                var contentCursor = cursor.FirstOrDefault().First().Value.AsBsonArray;

                foreach (BsonValue content in contentCursor)
                {
                    group.addMessage(new Message(content["Sender"].AsString, content["Message"].AsString, content["Time"].AsInt32));
                    Console.WriteLine($"{content["Sender"].AsString} | {content["Message"].AsString} ");
                }
            }
            else
            {
                group = null;
            }

            return group;
        }
        public async Task SavePrivateMessage(Message message, string groupname)
        {
            var options = new UpdateOptions { IsUpsert = true };
            await groups.UpdateOneAsync("{ GroupName: \"" + groupname + "\", Type: \"Private\" }", "{ $addToSet: { Content: { Sender: \"" + message.sender + "\", Time: " + message.timeSent + ", Message: \"" + message.msg + "\"} } }", options);
        }

        public async Task<List<string>> GetUsers()
        {
            List<string> userList = new();
            var cursor = await users.FindAsync(_ => true);

            await cursor.ForEachAsync(db =>
            {
                userList.Add(db["Name"].AsString);
            });

            return userList;
        }

        public Task<IAsyncCursor<BsonDocument>> GetUsersCursor()
        {

            //var filter = Builders<BsonDocument>.Filter.Eq("IsActive", "true");
            //string Users = users.FindAsync(filter).;


            return users.FindAsync(_ => true);

        }
        public async Task<List<string>> GetGroups(string user)
        {
            List<string> list = new List<string>();

            var cursor = await groups.FindAsync("{Members:{$elemMatch: {$eq:\"" + user + "\"}}}");

            await cursor.ForEachAsync(db => list.Add(db["GroupName"].AsString));
            return list;

        }
        public Task LogoutAll()
        {
            var filter = Builders<BsonDocument>.Filter.Eq("IsActive", "true");
            var update = Builders<BsonDocument>.Update.Set("IsActive", "false");
                
            return users.UpdateManyAsync(filter, update);
        }

    }
}
