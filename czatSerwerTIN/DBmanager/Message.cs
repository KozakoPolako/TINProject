using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace czatSerwerTIN.DBmanager
{
    /// <summary>
    /// Klasa odpowiadająca za przechowywanie informacji o pojedynczej wiadomości
    /// </summary>
    public class Message
    {
        /// <summary>
        /// Nazwa nadawcy
        /// </summary>
        public string sender { get; set; }
        /// <summary>
        /// Czas wysłania wiadomości, jeśli obiekt będzie składowany w bazie, będzie to zapis liczby sekund w formacie UnixTimeSeconds.
        /// jeśli obiekt będzie odczytywany z bazy i wysyłany do klienta, będzie to zapis odczytywalny przez człowieka
        /// </summary>
        public string timeSent { get; set; }
        /// <summary>
        /// Treść wiadomości
        /// </summary>
        public string msg { get; set; }
        /// <summary>
        /// Typ wiadomości, możliwe typy: <c>{"Text","Video","Image","Audio"}</c>
        /// </summary>
        public string type { get; set; }

        /// <summary>
        /// Konstruktor używany w przypadku odczytywania wiadomości z bazy danych
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="msg"></param>
        /// <param name="type"></param>
        /// <param name="unixTime"></param>
        public Message(string sender, string msg, string type, long unixTime )
        {
            this.sender = sender;
            this.msg = msg;
            DateTime time = DateTimeOffset.FromUnixTimeSeconds(unixTime).LocalDateTime;
            this.timeSent = $"{time.ToShortDateString()} {time.ToShortTimeString()}";
            this.type = type;
        }

        /// <summary>
        /// Konstruktor używany w przypadku odebrania wiadomości od klienta i zapisu do bazy danych,
        /// konstruktor sam generuje sobie obiekt czasu Now
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="msg"></param>
        /// <param name="type"></param>
        public Message(string sender, string msg, string type)
        {
            this.sender = sender;
            this.msg = msg;
            this.timeSent = DateTimeOffset.Now.ToUnixTimeSeconds().ToString();
            this.type = type;
        }

        /// <summary>
        /// Funkcja służąca do konwersji formatu sekund unixowych do formatu czytalnego przez człowieka,
        /// używana w przypadku wysyłania wiadomości ledwo odebranej do adresatów.
        /// </summary>
        public void convertTimeSentToDateFormat()
        {
            DateTime time = DateTimeOffset.FromUnixTimeSeconds(long.Parse(timeSent)).LocalDateTime;
            this.timeSent = $"{time.ToShortDateString()} {time.ToShortTimeString()}";
        }
    }
}
