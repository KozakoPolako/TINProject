using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace czatSerwerTIN.Structures
{
    class GroupInfo
    {
        /// <summary>
        /// Ciąg znakowy reprezentujący nazwę grupy/pokoju czatowego
        /// </summary>
        public string groupName { get; set; }
        /// <summary>
        /// Lista przechowująca obiekty informacji o użytkownikach, którzy należą do grupy
        /// </summary>
        public List<UserInfo> userInfoList { get; set; }

        /// <summary>
        /// <para>Ciąg znakowy reprezentujący typ grupy/pokoju czatowego</para>
        /// <para><c>private</c> - grupa prywatna między dwoma użytkownikami</para>
        /// <para><c>public</c> - grupa zwykła</para>
        /// </summary>
        public string groupType { get; set; }

        public GroupInfo(string groupName, string groupType)
        {
            this.groupName = groupName;
            this.groupType = groupType;
            userInfoList = new();
        }

        /// <summary>
        /// Funkcja sprawdzająca czy w danej grupie istnieje użytkownik o podanej nazwie
        /// </summary>
        /// <param name="userName">Ciąg znakowy reprezentujący nazwę użytkownika</param>
        /// <returns></returns>
        public bool hasUser(string userName)
        {
            return userInfoList.Exists(x => x.userName.Equals(userName));
        }

        /// <summary>
        /// Funkcja sprawdzająca czy w danej grupie znakduje się połączenie do dowolnego użytkownika o podanym ID
        /// </summary>
        /// <param name="connID">Ciąg znakowy reprezentujący ID połączenia danego klienta</param>
        /// <returns></returns>
        public bool hasConnectionID(string connID)
        {
            return userInfoList.Exists(x => x.hasConnectionID(connID));
        }

        /// <summary>
        /// Funkcja zwraca liczbę aktywnych połączeń w ramach danej grupy
        /// </summary>
        /// <returns></returns>
        public int activeUsersCount()
        {
            int count = 0;
            userInfoList.ForEach(u => count += u.activeUsersCount());
            return count;
        }
    }
}
