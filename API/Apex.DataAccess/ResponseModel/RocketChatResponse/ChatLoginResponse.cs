using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Apex.DataAccess.ResponseModel.RocketChatResponse
{
    public class ChatLoginResponse
    {
        public string status { get; set; }
        public Data data { get; set; }

        public class Password
        {
            public string bcrypt { get; set; }
        }

        public class Email2fa
        {
            public bool enabled { get; set; }
        }

        public class Services
        {
            public Password password { get; set; }
            public Email2fa email2fa { get; set; }
        }

        public class Email
        {
            public string address { get; set; }
            public bool verified { get; set; }
        }

        public class Alert5fd28c37f5204d0905436930
        {
            public string id { get; set; }
            public int priority { get; set; }
            public string title { get; set; }
            public string text { get; set; }
            public List<object> textArguments { get; set; }
            public List<object> modifiers { get; set; }
            public string link { get; set; }
        }

        public class Banners
        {
            [JsonProperty("alert5fd28c37f5204d0905436930")]
            public Alert5fd28c37f5204d0905436930 Alert5fd28c37f5204d0905436930 { get; set; }
        }

        public class Preferences
        {
            public bool enableAutoAway { get; set; }
            public int idleTimeLimit { get; set; }
            public bool desktopNotificationRequireInteraction { get; set; }
            public string audioNotifications { get; set; }
            public string desktopNotifications { get; set; }
            public string mobileNotifications { get; set; }
            public bool unreadAlert { get; set; }
            public bool useEmojis { get; set; }
            public bool convertAsciiEmoji { get; set; }
            public bool autoImageLoad { get; set; }
            public bool saveMobileBandwidth { get; set; }
            public bool collapseMediaByDefault { get; set; }
            public bool hideUsernames { get; set; }
            public bool hideRoles { get; set; }
            public bool hideFlexTab { get; set; }
            public bool hideAvatars { get; set; }
            public bool sidebarGroupByType { get; set; }
            public string sidebarViewMode { get; set; }
            public bool sidebarHideAvatar { get; set; }
            public bool sidebarShowUnread { get; set; }
            public string sidebarSortby { get; set; }
            public bool showMessageInMainThread { get; set; }
            public bool sidebarShowFavorites { get; set; }
            public string sendOnEnter { get; set; }
            public int messageViewMode { get; set; }
            public string emailNotificationMode { get; set; }
            public string newRoomNotification { get; set; }
            public string newMessageNotification { get; set; }
            public bool muteFocusedConversations { get; set; }
            public int notificationsSoundVolume { get; set; }
            public bool sidebarShowDiscussion { get; set; }
        }

        public class Settings
        {
            public Preferences preferences { get; set; }
        }

        public class Me
        {
            public string _id { get; set; }
            public Services services { get; set; }
            public List<Email> emails { get; set; }
            public string status { get; set; }
            public bool active { get; set; }
            public DateTime _updatedAt { get; set; }
            public List<string> roles { get; set; }
            public string name { get; set; }
            public string statusConnection { get; set; }
            public string username { get; set; }
            public double utcOffset { get; set; }
            public Banners banners { get; set; }
            public string avatarUrl { get; set; }
            public Settings settings { get; set; }
        }

        public class Data
        {
            public string userId { get; set; }
            public string authToken { get; set; }
            public Me me { get; set; }
        }
    }
}