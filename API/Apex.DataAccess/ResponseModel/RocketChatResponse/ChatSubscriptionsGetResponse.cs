using System;
using System.Collections.Generic;

namespace Apex.DataAccess.ResponseModel.RocketChatResponse
{
    public class ChatSubscriptionsGetResponse
    {
        public List<updates> update { get; set; }
        public List<object> remove { get; set; }
        public bool success { get; set; }

        public class U
        {
            public string _id { get; set; }
            public string username { get; set; }
            public string name { get; set; }
        }

        public class updates
        {
            public string _id { get; set; }
            public bool open { get; set; }
            public bool alert { get; set; }
            public int unread { get; set; }
            public int userMentions { get; set; }
            public int groupMentions { get; set; }
            public string rid { get; set; }
            public string name { get; set; }
            public U u { get; set; }
            public DateTime _updatedAt { get; set; }
            public string fname { get; set; }
            public string UCodeRole { get; set; }
        }
    }
}