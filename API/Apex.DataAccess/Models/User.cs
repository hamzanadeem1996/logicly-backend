using Apex.DataAccess.Repositories;
using NPoco;
using System;
using System.Text.Json.Serialization;

namespace Apex.DataAccess.Models
{
    [TableName("Users")]
    [PrimaryKey("Id")]
    public class User
    {
        [JsonIgnore] public int Id { get; set; }
        public int AgencyId { get; set; }
        public int RoleId { get; set; }
        [JsonIgnore] [ResultColumn] public string RoleName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PhoneNumber { get; set; }

        [JsonIgnore] [ResultColumn] public string MaxSessionHours { get; set; }
        public int CountryCode { get; set; }

        public string CityName { get; set; }
        [JsonIgnore] [ResultColumn] public int CountryId { get; set; }
        [JsonIgnore] [ResultColumn] public int StateId { get; set; }

        public int CityId { get; set; }
        public string Address { get; set; }
        public double Lat { get; set; }
        public double Long { get; set; }
        [JsonIgnore] [ResultColumn] public double AgencyLatitude { get; set; }

        [JsonIgnore] [ResultColumn] public double AgencyLongitude { get; set; }

        [JsonIgnore] public string Token { get; set; }

        [ResultColumn]
        public string FullName
        {
            get
            {
                char[] charsToTrim = { ' ' };
                return ($"{FirstName} {LastName}").Trim(charsToTrim);
            }
        }

        [JsonIgnore] public string RcUserId { get; set; }
        [JsonIgnore] public string RcUserName { get; set; }
        [JsonIgnore] public string RcPassword { get; set; }

        [JsonIgnore] [ResultColumn] public string PlanName { get; set; }

        [JsonIgnore] [ResultColumn] public string AgencyName { get; set; }

        [JsonIgnore]
        [ResultColumn]
        public bool HasPaymentMethod
        {
            get
            {
                var getCard = new CardRepo().Get(AgencyId);
                return getCard.CardNumber != null;
            }
        }

        [JsonIgnore]
        [ResultColumn]
        public string PaymentMessage
        {
            get
            {
                return (HasPaymentMethod) ? "" : Utility.HASPAYMENTMESSAGE;
            }
        }

        [JsonIgnore] public int AddedBy { get; set; }
        [JsonIgnore] public int LastModBy { get; set; }
        [JsonIgnore] public DateTime AddedOn { get; set; }
        [JsonIgnore] public DateTime LastModOn { get; set; }

        [JsonIgnore] [ResultColumn] public string IncludeWeekendsInWeekView { get; set; }

        [JsonIgnore]
        [ResultColumn]
        public bool IncludeWeekendsInWeekViewEnabled => IncludeWeekendsInWeekView !=null && IncludeWeekendsInWeekView.ToUpper().Trim() == "YES";
    }

    public class Login
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class AuthToken
    {
        public int Id { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string FullName
        {
            get
            {
                char[] charsToTrim = { ' ' };
                return ($"{FirstName} {LastName}").Trim(charsToTrim);
            }
        }

        public string Email { get; set; }
        public string Password { get; set; }
        public double Lat { get; set; }
        public double Long { get; set; }
        public double AgencyLatitude { get; set; }
        public double AgencyLongitude { get; set; }
        public string Token { get; set; }
        public string RcUserId { get; set; }
        public string RcUserName { get; set; }
        public string RcPassword { get; set; }
    }
}