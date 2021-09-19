using System.Collections.Generic;

namespace Apex.DataAccess.ResponseModel.RocketChatResponse
{
    public class ChatOnlineChannelResponse
    {
        public List<Online> online { get; set; }
        public bool success { get; set; }

        public class Online
        {
            public string _id { get; set; }
            public string username { get; set; }
        }
    }
}