using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace czatSerwerTIN.DBmanager
{
    /// <summary>
    /// Klasa do przechowywania informacji o konwersacji wraz z jej historią wiadomości
    /// </summary>
    public class Group
    {
        /// <summary>
        /// Nazwa grupy
        /// </summary>
        public string groupName { get; }
        /// <summary>
        /// Typ grupy, Private lub Public, określa czy to normalna konwersacja, czy prywatna
        /// </summary>
        public string groupType { get; }
        /// <summary>
        /// Lista obiektów Klasy Message reprezentująca historię wiadomości
        /// </summary>
        public List<Message> content { get; set; }

        public Group(string groupName, string groupType)
        {
            this.groupName = groupName;
            this.groupType = groupType;
            content = new();
        }

        /// <summary>
        /// Dodanie wiadomości podając wartości bezpośrednio
        /// </summary>
        /// <param name="sender">Nadawca</param>
        /// <param name="msg">Treść wiadomości</param>
        /// <param name="type">Typ wiadomości</param>
        public void addMessage(string sender, string msg,string type)
        {
            content.Add(new Message(sender, msg, type));
        }

        /// <summary>
        /// Dodanie obiektu wiadomości do historii
        /// </summary>
        /// <param name="msg">Obiekt wiadomości</param>
        public void addMessage(Message msg)
        {
            content.Add(msg);
        }

        /// <summary>
        /// Dodanie listy wiadomości do historii, obecnie nieużywane
        /// </summary>
        /// <param name="messages"></param>
        public void addMultipleMessages(List<Message> messages)
        {
            content.AddRange(messages);
        }

    }
}
