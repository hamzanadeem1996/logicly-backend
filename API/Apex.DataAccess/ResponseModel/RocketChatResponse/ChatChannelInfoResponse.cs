using System;

namespace Apex.DataAccess.ResponseModel.RocketChatResponse
{
    public class ChatChannelInfoResponse
    {
        public Channel channel { get; set; }
        public bool success { get; set; }

        public class U
        {
            public string _id { get; set; }
            public string username { get; set; }
        }

        public class CustomFields
        {
        }

        public class Channel
        {
            public string _id { get; set; }
            public string name { get; set; }
            public string fname { get; set; }
            public string t { get; set; }
            public int msgs { get; set; }
            public int usersCount { get; set; }
            public U u { get; set; }
            public CustomFields customFields { get; set; }
            public DateTime ts { get; set; }
            public bool ro { get; set; }
            public bool @default { get; set; }
            public bool sysMes { get; set; }
            public DateTime _updatedAt { get; set; }
        }
    }
}