using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using czatSerwerTIN.DBmanager;
using Microsoft.AspNetCore.SignalR;
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

        public async Task GetUserList(string user)
        {
            
            await Clients.Caller.SendAsync("ReciveUserList", jsonlist);

        }
        public async Task Login(string userName)
        {   
           //Console.WriteLine("Działam =" + userName);
           await mongo.InsertUser(userName);
           
        }
    }
}
