using System;
using System.Collections.Generic;

namespace Apex.DataAccess.ResponseModel.RocketChatResponse
{
    public class ChatMessagesChannelResponse
    {
        public List<Message> messages { get; set; }
        public int count { get; set; }
        public int offset { get; set; }
        public int total { get; set; }
        public bool success { get; set; }

        public class U
        {
            public string _id { get; set; }
            public string username { get; set; }
        }

        public class Message
        {
            public string _id { get; set; }
            public string t { get; set; }
            public string rid { get; set; }
            public DateTime ts { get; set; }
            public string msg { get; set; }
            public U u { get; set; }
            public bool groupable { get; set; }
            public DateTime _updatedAt { get; set; }
        }
    }
}