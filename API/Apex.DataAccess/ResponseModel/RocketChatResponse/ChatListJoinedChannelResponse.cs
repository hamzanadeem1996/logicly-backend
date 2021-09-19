using System;
using System.Collections.Generic;

namespace Apex.DataAccess.ResponseModel.RocketChatResponse
{
    public class ChatListJoinedChannelResponse
    {
        public List<Channel> channels { get; set; }
        public int offset { get; set; }
        public int count { get; set; }
        public int total { get; set; }
        public bool success { get; set; }

        public class U
        {
            public string _id { get; set; }
            public string username { get; set; }
            public string name { get; set; }
        }

        public class CustomFields
        {
        }

        public class LastMessage
        {
            public string rid { get; set; }
            public string msg { get; set; }
            public DateTime ts { get; set; }
            public U u { get; set; }
            public string _id { get; set; }
            public DateTime _updatedAt { get; set; }
            public List<object> mentions { get; set; }
            public List<object> channels { get; set; }
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
            public List<object> usernames { get; set; }
            public LastMessage lastMessage { get; set; }
            public DateTime? lm { get; set; }
        }
    }
}