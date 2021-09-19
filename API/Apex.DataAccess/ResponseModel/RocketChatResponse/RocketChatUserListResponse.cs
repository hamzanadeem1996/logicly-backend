using System.Collections.Generic;

namespace Apex.DataAccess.ResponseModelRocketChatResponse
{
    public class RocketChatUserListResponse
    {
        public class User
        {
            public string _id { get; set; }
            public string type { get; set; }
            public string status { get; set; }
            public bool active { get; set; }
            public string name { get; set; }
            public decimal utcOffset { get; set; }
            public string username { get; set; }
        }

        public class Root
        {
            public List<User> users { get; set; }
            public int count { get; set; }
            public int offset { get; set; }
            public int total { get; set; }
            public bool success { get; set; }
        }
    }
}