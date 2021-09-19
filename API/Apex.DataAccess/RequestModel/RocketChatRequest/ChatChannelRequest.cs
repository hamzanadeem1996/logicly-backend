namespace Apex.DataAccess.RequestModel.RocketChatRequest
{
    public class ChatChannelCreateRequest
    {
        public string name { get; set; }
    }

    public class ChatChannelRoomIdRequest
    {
        public string roomId { get; set; }
    }

    public class ChatChannelDeleteRequest
    {
        public string roomName { get; set; }
    }

    public class ChatChannelInviteRequest
    {
        public string roomId { get; set; }
        public string userId { get; set; }
    }

    public class ChatChannelJoinRequest
    {
        public string roomId { get; set; }
        public string joinCode { get; set; }
    }

    public class ChatChannelRenameRequest
    {
        public string roomId { get; set; }
        public string name { get; set; }
    }

    public class ChatChannelReadOnlyRequest
    {
        public string roomId { get; set; }
        public bool readOnly { get; set; }
    }
}