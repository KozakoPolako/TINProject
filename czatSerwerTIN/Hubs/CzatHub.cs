using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using czatSerwerTIN.DBmanager;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;
using MongoDB.Driver;

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

        public async Task GetActiveUsers()
        {
            List<User> list = new List<User>();
            
            var cursor = await mongo.GetActiveUsers();
            await cursor.ForEachAsync(db => list.Add(new User(db["Name"].AsString, db["ConnID"].AsString, db["IsActive"].ToBoolean())));
            
            var json = JsonSerializer.Serialize(list);
            
            await Clients.Caller.SendAsync("ReciveUserList", json );
        }
        public async Task Login(string userName)
        {   
           //Console.WriteLine("Działam =" + userName);
           await mongo.InsertUser(userName, Context.ConnectionId);
           
        }

        public override async  Task OnConnectedAsync()
        {   
            
            await base.OnConnectedAsync();
            

        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await mongo.LogoutUser(Context.ConnectionId);
            
            await base.OnDisconnectedAsync(exception);
            
        }
    }
}
