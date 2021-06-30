using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace czatSerwerTIN.DBmanager
{

    /// <summary>
    /// Klasa-kontroler dla połączenia z bazą danych
    /// </summary>
    public class MongoConnect
    {
        string CONNECTION_STRING = "mongodb://darekddd:Password1!@localhost:2717";
        //string CONNECTION_STRING = "mongodb://localhost:2717";
        MongoClient client;
        IMongoDatabase database;
        IMongoCollection<BsonDocument> users;
        IMongoCollection<BsonDocument> groups;

        /// <summary>
        /// Publiczny konstruktor, przy utworzeniu obiektu inicjalizuje połączenie
        /// i pobiera z bazy danych listę użytkowników i grup
        /// </summary>
        public MongoConnect()
        {
            client = new MongoClient(CONNECTION_STRING);
            database = client.GetDatabase("CzatDB");
            users = database.GetCollection<BsonDocument>("users");
            groups = database.GetCollection<BsonDocument>("groups");
        }

        /// <summary>
        /// Gdy nie istnieje dokument o kluczu <c>Name = name</c>, dodaje nowego użytkownika
        /// do bazy danych
        /// </summary>
        /// <param name="name">Nazwa dodawanego użytkownika</param>
        /// <param name="password">Hasło (plaintext) nowego użytkownika</param>
        /// <returns>Zadanie zwraca boolean w zależności od powodzenia operacji</returns>
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

        /// <summary>
        /// Dodawanie użytkownika do grupy, jeśli grupa nie istnieje w bazie danych,
        /// zostaje utworzona
        /// </summary>
        /// <param name="name">Nazwa dodawanego użytkownika</param>
        /// <param name="groupName">Nazwa grupy docelowej</param>
        /// <returns>Zadanie</returns>
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

        /// <summary>
        /// Zapisuje wiadomość wysłaną do grupy <c>groupName</c> w bazie danych
        /// </summary>
        /// <param name="message">Treść wysłanej i zapisywanej wiadomości</param>
        /// <param name="groupname">Nazwa grupy, w której czacie dodano wiadomość</param>
        /// <returns>Zadanie</returns>
        public async Task SaveGroupMessage(Message message, string groupname)
        {
            await groups.UpdateOneAsync("{ GroupName:\"" + groupname + "\", Type: \"Public\" }", "{ $addToSet: { Content: { Sender: \"" + message.sender + "\", Time: " + message.timeSent + ", Type: \"" + message.type + "\", Message: \"" + message.msg + "\"} } }");
        }

        /// <summary>
        /// Wczytuje wszystkie wiadomosci z danego <c>groupName</c> z bazy danych
        /// </summary>
        /// <param name="groupName">Nazwa grupy, o której historię wiadomości pytana jest baza danych</param>
        /// <param name="groupType">private lub public: Określa czy konwersacja jest grupowa, czy prywatna</param>
        /// <returns>Zadanie zwraca obiekt zawierający wiadomości dla grupy</returns>
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
            var cursor = (await groups.FindAsync(filter,options)).FirstOrDefault();
            
            if (cursor == null)
            {
                return null;
            }
            var contentCursor = cursor.FirstOrDefault();
            if (contentCursor.Value == null)
            {
                return null;
            }
            foreach (BsonValue content in contentCursor.Value.AsBsonArray)
            {
                group.addMessage(new Message(content["Sender"].AsString, content["Message"].AsString, content["Type"].AsString, content["Time"].AsInt32));
                Console.WriteLine($"{content["Sender"].AsString} | {content["Message"].AsString} ");
            }
            return group;
        }

        /// <summary>
        /// Zapisuje wiadomośc do grupy dwuosobowej (prywatną), składającej się z użytkownika
        /// wysyłającego i innego użytkownika czatu
        /// </summary>
        /// <param name="message">Treść wysłanej wiadomości</param>
        /// <param name="groupname">Nazwa grupy dwóch użytkowników</param>
        /// <returns>Zadanie</returns>
        public async Task SavePrivateMessage(Message message, string groupname)
        {
            var options = new UpdateOptions { IsUpsert = true };
            await groups.UpdateOneAsync("{ GroupName: \"" + groupname + "\", Type: \"Private\" }", "{ $addToSet: { Content: { Sender: \"" + message.sender + "\", Time: " + message.timeSent +", Type: \"" + message.type + "\", Message: \"" + message.msg + "\"} } }", options);
        }

        /// <summary>
        /// Pobierz listę zarejestrowanych użytkowników czatu z bazy danych
        /// </summary>
        /// <returns>Listę stringów użytkowników</returns>
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

        /// <summary>
        /// DEPRACATED
        /// zwraca kursor użytkownika
        /// </summary>
        /// <returns></returns>
        public Task<IAsyncCursor<BsonDocument>> GetUsersCursor()
        {

            //var filter = Builders<BsonDocument>.Filter.Eq("IsActive", "true");
            //string Users = users.FindAsync(filter).;


            return users.FindAsync(_ => true);

        }

        /// <summary>
        /// Zwraca nazwy konwersacji, w których znajduje się użytkownik
        /// </summary>
        /// <param name="user">Użytkownik, którego szukane są grupy</param>
        /// <returns>Lista użytkowników w postaci stringów</returns>
        public async Task<List<string>> GetGroups(string user)
        {
            List<string> list = new List<string>();

            var cursor = await groups.FindAsync("{Members:{$elemMatch: {$eq:\"" + user + "\"}}}");

            await cursor.ForEachAsync(db => list.Add(db["GroupName"].AsString));
            return list;

        }

        /// <summary>
        /// Wylogowuje wszystkich użytkowników czatu
        /// </summary>
        /// <returns>Zadanie</returns>
        public Task LogoutAll()
        {
            var filter = Builders<BsonDocument>.Filter.Eq("IsActive", "true");
            var update = Builders<BsonDocument>.Update.Set("IsActive", "false");
                
            return users.UpdateManyAsync(filter, update);
        }

    }
}
