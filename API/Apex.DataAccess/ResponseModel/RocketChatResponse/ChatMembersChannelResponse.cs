using System.Collections.Generic;

namespace Apex.DataAccess.ResponseModel.RocketChatResponse
{
    public class ChatMembersChannelResponse
    {
        public List<Member> members { get; set; }
        public int count { get; set; }
        public int offset { get; set; }
        public int total { get; set; }
        public bool success { get; set; }

        public class Member
        {
            public string _id { get; set; }
            public string status { get; set; }
            public string name { get; set; }
            public string username { get; set; }
            public double utcOffset { get; set; }
        }
    }
}