using System;
using System.Collections.Generic;

namespace Apex.DataAccess.ResponseModel.RocketChatResponse
{
    public class ChatUserResponse
    {
        public User user { get; set; }
        public bool success { get; set; }

        public class Email
        {
            public string address { get; set; }
            public bool verified { get; set; }
        }

        public class Settings
        {
        }

        public class User
        {
            public string _id { get; set; }
            public DateTime createdAt { get; set; }
            public string username { get; set; }
            public List<Email> emails { get; set; }
            public string type { get; set; }
            public string status { get; set; }
            public bool active { get; set; }
            public DateTime _updatedAt { get; set; }
            public List<string> __rooms { get; set; }
            public List<string> roles { get; set; }
            public string name { get; set; }
            public Settings settings { get; set; }
        }
    }
}