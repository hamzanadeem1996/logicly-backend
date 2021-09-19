using Apex.DataAccess.Models;
using Apex.DataAccess.RequestModel.RocketChatRequest;
using Apex.DataAccess.ResponseModel.RocketChatResponse;
using Apex.DataAccess.ResponseModelRocketChatResponse;
using Newtonsoft.Json;
using RestSharp;
using StackExchange.Profiling.Internal;

namespace Apex_Api.Service
{
    public static class RocketChatService
    {
        private static string ServerUrl => "https://chat1.logicly.ai/";
        private static string AdminId => "chat.admin";
        private static string AdminPassword => "hUmjNpuq6A";

        private static string _adminAuthtoken;

        private static string AdminAuthToken
        {
            get
            {
                if (_adminAuthtoken.IsNullOrWhiteSpace())
                    _adminAuthtoken = Login(AdminId, AdminPassword).data.authToken;
                return _adminAuthtoken;
            }
        }

        public static object Info()
        {
            var restClient = new RestClient(ServerUrl);
            var request = new RestRequest("api/info", Method.GET);
            var response = restClient.Execute(request);
            return JsonConvert.DeserializeObject<object>(response.Content);
        }

        public static ChatLoginResponse Login(string username, string password)
        {
            var restClient = new RestClient(ServerUrl);
            var request = new RestRequest("api/v1/login", Method.POST);
            request.AddHeader("Content-type", "application/json");
            request.AddJsonBody(new ChatLoginRequest { password = password, user = username });
            var response = restClient.Execute(request);
            return JsonConvert.DeserializeObject<ChatLoginResponse>(response.Content);
        }

        public static ChatLoginResponse GetAdmin()
        {
            return Login(AdminId, AdminPassword);
        }

        public static ChatUserResponse CreateUser(ChatUserCreateRequest chatUserCreateRequest)
        {
            var restClient = new RestClient(ServerUrl);
            var request = new RestRequest("api/v1/users.create", Method.POST);
            request.AddHeader("X-Auth-Token", AdminAuthToken);
            request.AddHeader("X-User-Id", Constant.AdminChatUserId);
            request.AddHeader("Content-type", "application/json");
            request.AddJsonBody(chatUserCreateRequest);
            var response = restClient.Execute(request);
            return JsonConvert.DeserializeObject<ChatUserResponse>(response.Content);
        }

        public static ChatSuccessResponse DeleteUser(ChatUserDeleteRequest chatUserDeleteRequest)
        {
            var restClient = new RestClient(ServerUrl);
            var request = new RestRequest("api/v1/users.delete", Method.POST);
            request.AddHeader("X-Auth-Token", AdminAuthToken);
            request.AddHeader("X-User-Id", Constant.AdminChatUserId);
            request.AddHeader("Content-type", "application/json");
            request.AddJsonBody(chatUserDeleteRequest);
            var response = restClient.Execute(request);
            return JsonConvert.DeserializeObject<ChatSuccessResponse>(response.Content);
        }

        public static ChatChannelInfoResponse CreateChannel(ChatChannelCreateRequest chatChannelCreateRequest)
        {
            var restClient = new RestClient(ServerUrl);
            var request = new RestRequest("api/v1/channels.create", Method.POST);
            request.AddHeader("X-Auth-Token", AdminAuthToken);
            request.AddHeader("X-User-Id", Constant.AdminChatUserId);
            request.AddHeader("Content-type", "application/json");
            request.AddJsonBody(chatChannelCreateRequest);
            var response = restClient.Execute(request);
            return JsonConvert.DeserializeObject<ChatChannelInfoResponse>(response.Content);
        }

        public static ChatChannelCounterResponse CounterChannel(string roomId)
        {
            var restClient = new RestClient(ServerUrl);
            var request = new RestRequest("api/v1/channels.counters", Method.GET);
            request.AddHeader("X-Auth-Token", AdminAuthToken);
            request.AddHeader("X-User-Id", Constant.AdminChatUserId);
            request.AddHeader("Content-type", "application/json");
            request.AddQueryParameter("roomId", roomId);
            var response = restClient.Execute(request);
            return JsonConvert.DeserializeObject<ChatChannelCounterResponse>(response.Content);
        }

        public static ChatSuccessResponse CloseChannel(ChatChannelRoomIdRequest chatChannelRoomIdRequest)
        {
            var restClient = new RestClient(ServerUrl);
            var request = new RestRequest("api/v1/channels.close", Method.POST);
            request.AddHeader("X-Auth-Token", AdminAuthToken);
            request.AddHeader("X-User-Id", Constant.AdminChatUserId);
            request.AddHeader("Content-type", "application/json");
            request.AddJsonBody(chatChannelRoomIdRequest);
            var response = restClient.Execute(request);
            return JsonConvert.DeserializeObject<ChatSuccessResponse>(response.Content);
        }

        public static ChatSuccessResponse ArchiveChannel(ChatChannelRoomIdRequest chatChannelRoomIdRequest)
        {
            var restClient = new RestClient(ServerUrl);
            var request = new RestRequest("api/v1/channels.archive", Method.POST);
            request.AddHeader("X-Auth-Token", AdminAuthToken);
            request.AddHeader("X-User-Id", Constant.AdminChatUserId);
            request.AddHeader("Content-type", "application/json");
            request.AddJsonBody(chatChannelRoomIdRequest);
            var response = restClient.Execute(request);
            return JsonConvert.DeserializeObject<ChatSuccessResponse>(response.Content);
        }

        public static ChatSuccessResponse DeleteChannel(ChatChannelDeleteRequest chatChannelDeleteRequest)
        {
            var restClient = new RestClient(ServerUrl);
            var request = new RestRequest("api/v1/channels.delete", Method.POST);
            request.AddHeader("X-Auth-Token", AdminAuthToken);
            request.AddHeader("X-User-Id", Constant.AdminChatUserId);
            request.AddHeader("Content-type", "application/json");
            request.AddJsonBody(chatChannelDeleteRequest);
            var response = restClient.Execute(request);
            return JsonConvert.DeserializeObject<ChatSuccessResponse>(response.Content);
        }

        public static ChatChannelInfoResponse InfoChannel(string roomId)
        {
            var restClient = new RestClient(ServerUrl);
            var request = new RestRequest("api/v1/channels.info", Method.GET);
            request.AddHeader("X-Auth-Token", AdminAuthToken);
            request.AddHeader("X-User-Id", Constant.AdminChatUserId);
            request.AddHeader("Content-type", "application/json");
            request.AddQueryParameter("roomId", roomId);
            var response = restClient.Execute(request);
            return JsonConvert.DeserializeObject<ChatChannelInfoResponse>(response.Content);
        }

        public static ChatChannelInfoResponse InviteChannel(ChatChannelInviteRequest chatChannelInviteRequest)
        {
            var restClient = new RestClient(ServerUrl);
            var request = new RestRequest("api/v1/channels.invite", Method.POST);
            request.AddHeader("X-Auth-Token", AdminAuthToken);
            request.AddHeader("X-User-Id", Constant.AdminChatUserId);
            request.AddHeader("Content-type", "application/json");
            request.AddJsonBody(chatChannelInviteRequest);
            var response = restClient.Execute(request);
            return JsonConvert.DeserializeObject<ChatChannelInfoResponse>(response.Content);
        }

        public static ChatChannelInfoResponse JoinChannel(ChatChannelJoinRequest chatChannelJoinRequest)
        {
            var restClient = new RestClient(ServerUrl);
            var request = new RestRequest("api/v1/channels.join", Method.POST);
            request.AddHeader("X-Auth-Token", AdminAuthToken);
            request.AddHeader("X-User-Id", Constant.AdminChatUserId);
            request.AddHeader("Content-type", "application/json");
            request.AddJsonBody(chatChannelJoinRequest);
            var response = restClient.Execute(request);
            return JsonConvert.DeserializeObject<ChatChannelInfoResponse>(response.Content);
        }

        public static ChatChannelInfoResponse KickChannel(ChatChannelInviteRequest chatChannelInviteRequest)
        {
            var restClient = new RestClient(ServerUrl);
            var request = new RestRequest("api/v1/channels.kick", Method.POST);
            request.AddHeader("X-Auth-Token", AdminAuthToken);
            request.AddHeader("X-User-Id", Constant.AdminChatUserId);
            request.AddHeader("Content-type", "application/json");
            request.AddJsonBody(chatChannelInviteRequest);
            var response = restClient.Execute(request);
            return JsonConvert.DeserializeObject<ChatChannelInfoResponse>(response.Content);
        }

        public static object LeaveChannel(ChatChannelRoomIdRequest chatChannelRoomIdRequest)
        {
            var restClient = new RestClient(ServerUrl);
            var request = new RestRequest("api/v1/channels.leave", Method.POST);
            request.AddHeader("X-Auth-Token", AdminAuthToken);
            request.AddHeader("X-User-Id", Constant.AdminChatUserId);
            request.AddHeader("Content-type", "application/json");
            request.AddJsonBody(chatChannelRoomIdRequest);
            var response = restClient.Execute(request);
            return JsonConvert.DeserializeObject<object>(response.Content);
        }

        public static ChatListJoinedChannelResponse ListJoinedChannels(string userId, string authToken)
        {
            var restClient = new RestClient(ServerUrl);
            var request = new RestRequest("api/v1/channels.list.joined", Method.GET);
            request.AddHeader("X-Auth-Token", authToken);
            request.AddHeader("X-User-Id", userId);
            var response = restClient.Execute(request);
            return JsonConvert.DeserializeObject<ChatListJoinedChannelResponse>(response.Content);
        }

        public static ChatMembersChannelResponse MembersChannel(string roomId)
        {
            var restClient = new RestClient(ServerUrl);
            var request = new RestRequest("api/v1/channels.members", Method.GET);
            request.AddHeader("X-Auth-Token", AdminAuthToken);
            request.AddHeader("X-User-Id", Constant.AdminChatUserId);
            request.AddQueryParameter("roomId", roomId);
            var response = restClient.Execute(request);
            return JsonConvert.DeserializeObject<ChatMembersChannelResponse>(response.Content);
        }

        public static ChatMessagesChannelResponse MessagesChannel(string roomId)
        {
            var restClient = new RestClient(ServerUrl);
            var request = new RestRequest("api/v1/channels.messages", Method.GET);
            request.AddHeader("X-Auth-Token", AdminAuthToken);
            request.AddHeader("X-User-Id", Constant.AdminChatUserId);
            request.AddQueryParameter("roomId", roomId);
            var response = restClient.Execute(request);
            return JsonConvert.DeserializeObject<ChatMessagesChannelResponse>(response.Content);
        }

        public static ChatOnlineChannelResponse OnlineChannel()
        {
            var restClient = new RestClient(ServerUrl);
            var request = new RestRequest("api/v1/channels.online", Method.GET);
            request.AddHeader("X-Auth-Token", AdminAuthToken);
            request.AddHeader("X-User-Id", Constant.AdminChatUserId);
            var response = restClient.Execute(request);
            return JsonConvert.DeserializeObject<ChatOnlineChannelResponse>(response.Content);
        }

        public static ChatChannelInfoResponse RenameChannel(ChatChannelRenameRequest chatChannelRenameRequest, ref User me)
        {
            var restClient = new RestClient(ServerUrl);
            var request = new RestRequest("api/v1/channels.rename", Method.POST);
            request.AddHeader("X-Auth-Token", AdminAuthToken);
            request.AddHeader("X-User-Id", Constant.AdminChatUserId);
            request.AddHeader("Content-type", "application/json");
            request.AddJsonBody(chatChannelRenameRequest);
            var response = restClient.Execute(request);
            return JsonConvert.DeserializeObject<ChatChannelInfoResponse>(response.Content);
        }

        public static ChatSubscriptionsGetResponse SubscriptionsGet(ref User me)
        {
            var restClient = new RestClient(ServerUrl);
            var user = Login(me.RcUserName, me.RcPassword);
            var request = new RestRequest("api/v1/subscriptions.get", Method.GET);
            request.AddHeader("X-Auth-Token", user.data.authToken);
            request.AddHeader("X-User-Id", user.data.userId);
            request.AddHeader("Content-type", "application/json");
            var response = restClient.Execute(request);
            var res = JsonConvert.DeserializeObject<ChatSubscriptionsGetResponse>(response.Content);
            return res;
        }

        public static RocketChatUserListResponse.Root UserList(ref User me)
        {
            var restClient = new RestClient(ServerUrl);
            var user = Login(me.RcUserName, me.RcPassword);
            var request = new RestRequest("api/v1/users.list?count=500", Method.GET);
            request.AddHeader("X-Auth-Token", user.data.authToken);
            request.AddHeader("X-User-Id", user.data.userId);
            request.AddHeader("Content-type", "application/json");
            var response = restClient.Execute(request);
            return JsonConvert.DeserializeObject<RocketChatUserListResponse.Root>(response.Content);
        }
    }
}