using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using czatSerwerTIN.DBmanager;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;
using czatSerwerTIN.Structures;

namespace czatSerwerTIN.Hubs
{
    public class CzatHub : Hub
    {
        
        // zmienna tymczasowa służyła do testowania 
        string jsonlist = "[\"Darek\",\"Czarek\",\"Marek\"]";

        // połaczenie z baządanyc 
        MongoConnect mongo = new MongoConnect();

        static List<UserInfo> userInfoList = new();

        /// <summary>
        /// Funkcja zwraca liczbę aktywnych połączeń
        /// </summary>
        /// <returns></returns>
        private int activeConnectionsCount()
        {
            int count = 0;
            userInfoList.ForEach(u => count += u.activeUsersCount());
            return count;
        }

        private string getPrivateGroupName(string sender, string destination)
        {
            int strOrder = string.CompareOrdinal(sender, destination);
            string groupName = $"{sender}_{destination}";
            if (strOrder > 0)
            {
                groupName = $"{destination}_{sender}";
            }
            return groupName;
        }

        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public async Task GetUsers()
        {
            List<User> list = new List<User>();
            foreach(string userName in await mongo.GetUsers())
            {
                UserInfo usr = userInfoList.FirstOrDefault(u => u.userName.Equals(userName));
                list.Add(new User(userName, (usr != null).ToString()));
            }

            var json = JsonSerializer.Serialize(list);
            
            await Clients.Caller.SendAsync("ReceiveUserList", json );
        }

        public async Task getGroupsByUser(string userName)
        {
            var json = JsonSerializer.Serialize(await mongo.GetGroups(userName));

            await Clients.Caller.SendAsync("ReceiveGroupList", json);
        }
        public async Task Login(string userName, string password)
        {
            bool status = false;
            if( password != "")
            {
                status = await mongo.InsertUser(userName, password);
                if (status)
                {
                    UserInfo user = userInfoList.FirstOrDefault(u => u.userName.Equals(userName));
                    if (user == null)
                    {
                        user = new UserInfo(userName);
                        userInfoList.Add(user);
                    }
                    user.addConnectionID(Context.ConnectionId);
                    foreach(string groupName in await mongo.GetGroups(userName))
                    {
                        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
                    }
                }
            }
            await Clients.Caller.SendAsync("LoginStatus", status);
            //Console.WriteLine("Działam =" + userName);
        }

        public async Task AddUserToGroup(string name, string groupName) 
        {
            await mongo.AddUserToGroup(name, groupName);
            UserInfo usr = userInfoList.First(u => u.hasConnectionID(Context.ConnectionId));
            foreach(string connID in usr.connectionIDs)
            {
                await Groups.AddToGroupAsync(connID, groupName);
            }
        }
        public async Task RemoveUserFromGroup(string name, string groupName)
        {
            await mongo.RemoveUserFromGroup(name, groupName);
            UserInfo usr = userInfoList.First(u => u.hasConnectionID(Context.ConnectionId));
            foreach (string connID in usr.connectionIDs)
            {
                await Groups.RemoveFromGroupAsync(connID, groupName);
            }
        }

        public async Task SendPrivateMessage(string sender, string destination, string message)
        {
            Message msg = new Message(sender, message);
            await mongo.SavePrivateMessage(msg, getPrivateGroupName(sender, destination));
            msg.convertTimeSentToDateFormat();
            var json = JsonSerializer.Serialize(msg);
            UserInfo source = userInfoList.FirstOrDefault(u => u.userName.Equals(sender));
            if (source != null)
            {
                foreach (string connID in source.connectionIDs)
                {
                    await Clients.Client(connID).SendAsync("ReceivePrivateMessage", destination, json); //do nadawcy
                }
            }
            UserInfo dest = userInfoList.FirstOrDefault(u => u.userName.Equals(destination));
            if(dest != null)
            {
                foreach (string connID in dest.connectionIDs)
                {
                    await Clients.Client(connID).SendAsync("ReceivePrivateMessage", sender, json); //do adresata
                }
            }
            
        }

        public async Task SendMessageToGroup(string sender, string groupName, string message)
        {
            Message msg = new Message(sender, message);
            await mongo.SaveGroupMessage(msg, groupName);
            msg.convertTimeSentToDateFormat();
            var json = JsonSerializer.Serialize(msg);
            await Clients.Group(groupName).SendAsync("ReceiveMessage", groupName, json); 
            ////List<string> members = new List<string>();
            ////BsonArr
            //BsonArray members = new BsonArray();
            //var cursor =  await mongo.GetGroupUsers(group);

            //await cursor.ForEachAsync(db => members = db["Members"].AsBsonArray);
            //members.Values.ToList().ForEach(async (member) => await SendPrivateMessage(sender, member.AsString,message));
            //SendAsync("ReceiveMessage", destination, json)

        }

        public override async  Task OnConnectedAsync()
        {   
            
            await base.OnConnectedAsync();

        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {

            ////do zrobienia do funkcji logout user trzeba przekazać nazwę użytkownika !!!
            //await mongo.LogoutUser(Context.ConnectionId);
            UserInfo user = userInfoList.First(u => u.hasConnectionID(Context.ConnectionId));
            foreach (string groupName in await mongo.GetGroups(user.userName))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            }
            user.removeConnectionID(Context.ConnectionId);
            if(user.activeUsersCount() < 1)
            {
                userInfoList.Remove(user);
            }
            await GetUsers();
            await base.OnDisconnectedAsync(exception);
            
        }
        
        public async Task getMessagesByGroup(string groupName, string groupType)
        {
            string sender = userInfoList.First(u => u.hasConnectionID(Context.ConnectionId)).userName;
            Group group = await mongo.LoadMessagesByGroupName(groupType.Equals("Private") ? getPrivateGroupName(sender, groupName) : groupName, groupType);

            Console.WriteLine(JsonSerializer.Serialize(group));

            await Clients.Caller.SendAsync("ReceiveMessagesByGroup", JsonSerializer.Serialize(group));
        }

        //public async Task getGroupsByUser(string userName)
        //{
        //    List<string> list = new List<string>();

        //    foreach(GroupInfo g in groupInfoList)
        //    {
        //        if(g.users.Exists(u => u.Equals(userName)))
        //        {
        //            list.Add(g.groupName);
        //        }
        //    }

        //    var json = JsonSerializer.Serialize(list);

        //    await Clients.Caller.SendAsync("ReciveGroupList", json);
        //}
    }
}
