using System;

namespace Apex.DataAccess.ResponseModel.RocketChatResponse
{
    public class ChatChannelCounterResponse
    {
        public bool joined { get; set; }
        public int members { get; set; }
        public int unreads { get; set; }
        public DateTime unreadsFrom { get; set; }
        public int msgs { get; set; }
        public DateTime latest { get; set; }
        public int userMentions { get; set; }
        public bool success { get; set; }
    }
}