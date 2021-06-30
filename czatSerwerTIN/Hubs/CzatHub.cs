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

        // połaczenie z baządanyc 
        MongoConnect mongo = new MongoConnect();

        /// <summary>
        /// Lista aktywnych 
        /// </summary>
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
        /// <summary>
        /// <para>Funkcja wewnętrzna, która służy do generowania nazwy grupy dla konwersacji prywatnych.</para>
        /// <para>Z adresata i nadawcy wybierana jest ta nazwa, która jest większa w rozumieniu liczbowym wartości charów,
        /// a następnie nazwa generowana jest w schemacie pierwszy_drugi</para>
        /// <para>W celu ujednolicenia obiektów kownersacji uzywana jest jedna definicja i zakłada ona, że każda konwersacja
        /// ma nazwę grupy w bazie danych. Przy konwersacjach prywatnych istnieją tylko nazwy użytkowników.</para>
        /// </summary>
        /// <param name="sender">Nazwa nadawcy</param>
        /// <param name="destination">Nazwa adresata</param>
        /// <returns></returns>
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
        /// <summary>
        /// <para>DEPRECATED!!!</para>
        /// <para>Funkcja która przekazuje wiadomość do wszystkich</para>
        /// </summary>
        /// <param name="user"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
        /// <summary>
        /// Funkcja zwracająca listę użytkowników aplikacji wraz z oznaczeniem, którzy są aktywnie zalogowani.
        /// </summary>
        /// <returns></returns>
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
        /// <summary>
        /// Funkcja, która zwraca listę grup, w której znajduje się użytkownik wywołujący, z bazy danych.
        /// </summary>
        /// <param name="userName">Nazwa użytkownika</param>
        /// <returns></returns>
        public async Task getGroupsByUser(string userName)
        {
            var json = JsonSerializer.Serialize(await mongo.GetGroups(userName));

            await Clients.Caller.SendAsync("ReceiveGroupList", json);
        }
        /// <summary>
        /// Funkcja obsługująca logowanie użytkownika lub rejestrację w aplikacji. Przy rejestracji lub poprawnym logowaniu
        /// wysyłana jest wiadomosc z odpowiedzią True, przy błędnym haśle wysyłane jest False. W przyszłości planowane jest 
        /// zastąpienie hasła w plaintext na hash.
        /// </summary>
        /// <param name="userName">Nazwa użytkownika, którą podano w formularzu logowania</param>
        /// <param name="password">Hasło, które podano w formularzu logowaniu</param>
        /// <returns></returns>
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
        }
        /// <summary>
        /// Funkcja, której wywołanie przez klienta powoduje dodanie użytkownika, do którego nalezy połączenie wywołujące,
        /// do grupy o nazwie przesłanej w parametrze <c>groupName</c>. Użytkownik dodawany jest do grupy w bazie danych oraz
        /// do grupy SignalR.
        /// </summary>
        /// <param name="userName">Nazwa użytkownika wywołującego</param>
        /// <param name="groupName">Nazwa grupy</param>
        /// <returns></returns>
        public async Task AddUserToGroup(string userName, string groupName) 
        {
            await mongo.AddUserToGroup(userName, groupName);
            UserInfo usr = userInfoList.First(u => u.hasConnectionID(Context.ConnectionId));
            var json = JsonSerializer.Serialize(await mongo.GetGroups(userName));
            foreach (string connID in usr.connectionIDs)
            {
                await Groups.AddToGroupAsync(connID, groupName);
                await Clients.Client(connID).SendAsync("ReceiveGroupList", json);
            }
        }
        /// <summary>
        /// Funkcja, której wywołanie przez klienta powoduje usunięcie użytkownika, do którego nalezy połączenie wywołujące,
        /// z grupy o nazwie przesłanej w parametrze <c>groupName</c>. Użytkownik usuwany jest z grupy w bazie danych oraz
        /// z grupy SignalR.
        /// </summary>
        /// <param name="userName">Nazwa użytkownika wywołującego</param>
        /// <param name="groupName">Nazwa grupy</param>
        /// <returns></returns>
        public async Task RemoveUserFromGroup(string userName, string groupName)
        {
            await mongo.RemoveUserFromGroup(userName, groupName);
            UserInfo usr = userInfoList.First(u => u.hasConnectionID(Context.ConnectionId));
            var json = JsonSerializer.Serialize(await mongo.GetGroups(userName));
            foreach (string connID in usr.connectionIDs)
            {
                await Groups.RemoveFromGroupAsync(connID, groupName);
                await Clients.Client(connID).SendAsync("ReceiveGroupList", json);
            }
        }
        /// <summary>
        /// Funkcja, której wywołanie przez klienta powoduje wysłanie wiadomości prywatnej do użytkowników konwersacji prywatnej
        /// oraz zapisanie tej wiadomości w bazie danych we wpisie dotyczącym tej konwersacji. Wiadomość jest wysyłana jako
        /// zserializowany obiekt Message. Nazwa grupy jest generowana przez funkcję <c>getPrivateGroupName()</c>
        /// </summary>
        /// <param name="sender">Nazwa użytkownika nadawcy</param>
        /// <param name="destination">Nazwa użytkownika adresata</param>
        /// <param name="message">Treść wiadomości</param>
        /// <param name="type">Typ wiadomości, jeden z <c>{"Text","Video","Image","Audio"}</c></param>
        /// <returns></returns>
        public async Task SendPrivateMessage(string sender, string destination, string message, string type)
        {
            Message msg = new Message(sender, message, type);
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

        /// <summary>
        /// Funkcja, której wywołanie przez klienta powoduje wysłanie wiadomości do wszystkich uzytkowników grupy
        /// oraz zapisanie tej wiadomości w bazie danych we wpisie dotyczącym tej grupy. Wiadomość jest wysyłana jako
        /// zserializowany obiekt Message
        /// </summary>
        /// <param name="sender">Nazwa użytkownika nadawcy</param>
        /// <param name="groupName">Nazwa grupy docelowej</param>
        /// <param name="message">Treść wiadomości</param>
        /// <param name="type">Typ wiadomości, jeden z <c>{"Text","Video","Image","Audio"}</c></param>
        /// <returns></returns>
        public async Task SendMessageToGroup(string sender, string groupName, string message, string type)
        {
            Message msg = new Message(sender, message, type);
            await mongo.SaveGroupMessage(msg, groupName);
            msg.convertTimeSentToDateFormat();
            var json = JsonSerializer.Serialize(msg);
            await Clients.Group(groupName).SendAsync("ReceiveMessage", groupName, json); 
        }

        public override async  Task OnConnectedAsync()
        {   
            await base.OnConnectedAsync();
        }
        /// <summary>
        /// Funkcja nadpisująca standardową OnDisconnectedAsync, usuwa ona z listy aktywnych użytkowników ID
        /// odłączonego połączenia i jeśli dla danego użytkownika nie pozostało żadne ID połączenia, to użytkownik jest również usuwany z listy
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public override async Task OnDisconnectedAsync(Exception exception)
        {
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
        /// <summary>
        /// Funkcja, która na zapytanie klienta SignalR pobiera z bazy danych wszsytkie wiadomości 
        /// dla grupy określonej w parametrach, a następnie wysyła odpowiedź z zserializowanym obiektem "Grupy"
        /// </summary>
        /// <param name="groupName">Nazwa grupy lub nazwa użytkownika(w przypadku konwersacji prywatnej)</param>
        /// <param name="groupType">Typ grupy, Private lub Public</param>
        /// <returns></returns>
        public async Task getMessagesByGroup(string groupName, string groupType)
        {
            string sender = userInfoList.First(u => u.hasConnectionID(Context.ConnectionId)).userName;
            Group group = await mongo.LoadMessagesByGroupName(groupType.Equals("Private") ? getPrivateGroupName(sender, groupName) : groupName, groupType);
            if(group != null)
            {
                Console.WriteLine(JsonSerializer.Serialize(group));

                await Clients.Caller.SendAsync("ReceiveMessagesByGroup", JsonSerializer.Serialize(group));
            }
            
        }
    }
}
