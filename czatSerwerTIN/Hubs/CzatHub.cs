using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using czatSerwerTIN.DBmanager;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;
using MongoDB.Driver;
using MongoDB.Bson;

namespace czatSerwerTIN.Hubs
{
    public class CzatHub : Hub
    {
        
        // zmienna tymczasowa służyła do testowania 
        string jsonlist = "[\"Darek\",\"Czarek\",\"Marek\"]";

        // połaczenie z baządanyc 
        MongoConnect mongo = new MongoConnect();

        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public async Task GetUsers()
        {
            List<User> list = new List<User>();
            
            var cursor = await mongo.GetUsers();

            //TRZEBA NAPRAWIĆ IsActive powinno być boolean ale zawsze wtedy dawało true
            //await cursor.ForEachAsync(db => list.Add(new User(db["Name"].AsString, db["ConnID"].AsString, db["IsActive"].AsString)));
            await cursor.ForEachAsync(db => list.Add(new User(db["Name"].AsString, db["IsActive"].AsString)));
            var json = JsonSerializer.Serialize(list);
            
            await Clients.Caller.SendAsync("ReciveUserList", json );
        }

        public async Task GetGroups( string user)
        {
            List<string> list = new List<string>();

            var cursor = await mongo.GetGroups(user);

            
            await cursor.ForEachAsync(db => list.Add( db["GroupName"].AsString));

            var json = JsonSerializer.Serialize(list);

            await Clients.Caller.SendAsync("ReciveGroupList", json);
        }
        public async Task Login(string userName, string password)
        {
            bool status = false;
            if( password != "")
            {
                status = await mongo.InsertUser(userName, password);
                if (status)
                    await Groups.AddToGroupAsync(Context.ConnectionId, userName + "_user");
            }
            await Clients.Caller.SendAsync("LoginStatus", status);
            //Console.WriteLine("Działam =" + userName);


        }

        public async Task AddUserToGroup(string name, string grupname) 
        {
            await mongo.AddUserToGroup(name, grupname);
        }
        public async Task RemoveUserFromGroup(string name, string grupname)
        {
            await mongo.RemoveUserFromGroup(name, grupname);
        }

        public async Task SendPrivateMessage(string sender, string receiver, string message)
        {
            await Clients.Group(receiver + "_user").SendAsync("ReceiveMessage", sender, message);
           // await Clients.Group(sender + "_user").SendAsync("ReceiveMessage", sender, message);
        }

        public async Task SendMessageToGroup(string sender, string group, string message)
        {
            //List<string> members = new List<string>();
            //BsonArr
            BsonArray members = new BsonArray();
            var cursor =  await mongo.GetGroupUsers(group);

            await cursor.ForEachAsync(db => members = db["Members"].AsBsonArray);
            members.Values.ToList().ForEach(async (member) => await SendPrivateMessage(sender, member.AsString,message));

        }

        public override async  Task OnConnectedAsync()
        {   
            
            await base.OnConnectedAsync();
            

        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {

            //do zrobienia do funkcji logout user trzeba przekazać nazwę użytkownika !!!
            await mongo.LogoutUser(Context.ConnectionId);
            await GetUsers();
            await base.OnDisconnectedAsync(exception);
            
        }
    }
}
