using System;

namespace Apex.DataAccess.ResponseModel.RocketChatResponse
{
    public class RoomsResponse
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public int UnreadCount { get; set; }
        public string Role { get; set; }
        public bool IsBold { get; set; }
        public string RoomId { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool Alert { get; set; }
        public string ChatUsername { get; set; }
    }
}