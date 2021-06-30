using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace czatSerwerTIN.Models
{
    class models
    {
        public UserModel Caller { get; set; }
        public UserModel Callee { get; set; }
    }
    public class UserModel
    {
        public string Username { get; set; }
        public string ConnectionId { get; set; }
        public bool InCall { get; set; }
    }

    public class UserCall
    {
        public List<UserModel> Users { get; set; }
    }
}
