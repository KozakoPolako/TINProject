using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace czatSerwerTIN.Structures
{
    class UserInfo
    {
        /// <summary>
        /// Ciąg znakowy reprezentujący nazwę użytkownika w systemie
        /// </summary>
        public string userName { get; set; }
        /// <summary>
        /// Lista przechowująca ID połączeń SignalR dla aktywnych klientów zalogowanych do użytkownika
        /// </summary>
        public List<string> connectionIDs { get; set; }

        public UserInfo(string userName)
        {
            this.userName = userName;
            connectionIDs = new();
        }

        /// <summary>
        /// Funkcja dodaje ID połączenia klienta do listy ID połączeń przypisanej do danego obiektu użytkownika
        /// </summary>
        /// <param name="connID">Ciąg znakowy reprezentujący ID połączenia danego klienta</param>
        public void addConnectionID(string connID)
        {
            if(!connectionIDs.Contains(connID))
                connectionIDs.Add(connID);
        }

        /// <summary>
        /// Funkcja usuwa ID połączenia danego klienta z listy ID połączeń przypisanej do danego obiektu użytkownika
        /// </summary>
        /// <param name="connID">Ciąg znakowy reprezentujący ID połączenia danego klienta</param>
        public void removeConnectionID(string connID)
        {
            if (connectionIDs.Contains(connID))
                connectionIDs.Remove(connID);
        }

        /// <summary>
        /// Funkcja usuwa wszystkie wpisy connectionID dla danego obiektu użytkownika
        /// </summary>
        public void removeAllConnectionIDs()
        {
            connectionIDs.Clear();
        }

        /// <summary>
        /// Funkcja sprawdza czy w liście connectionID znajduje się podane w argumencie ID połączenia
        /// </summary>
        /// <param name="connID">Ciąg znakowy reprezentujący ID połączenia danego klienta</param>
        /// <returns>Zwraca boolean reprezentujący wynik zapytania</returns>
        public bool hasConnectionID(string connID)
        {
            return connectionIDs.Contains(connID);
        }

        /// <summary>
        /// Funkcja zwraca liczbę aktywnych połączeń w ramach danego użytkownika
        /// </summary>
        /// <returns></returns>
        public int activeUsersCount()
        {
            return connectionIDs.Count();
        }

    }
}
