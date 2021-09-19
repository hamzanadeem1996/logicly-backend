using NPoco;
using System;
using System.Text.Json.Serialization;

namespace Apex.DataAccess.Models
{
    [TableName("Cards")]
    [PrimaryKey("Id")]
    public class Card
    {
        public int Id { get; set; }
        [JsonIgnore] public int AgencyId { get; set; }
        public string CardHolderName { get; set; }
        public string CardNumber { get; set; }

        private string _cardType;

        [JsonIgnore]
        public string CardType
        {
            get => string.IsNullOrWhiteSpace(_cardType) ? Utility.FindType(CardNumber).ToString() : _cardType;
            set => _cardType = value;
        }

        public string ExpiryMonth { get; set; }
        public string ExpiryYear { get; set; }
        public string Cvv { get; set; }

        [JsonIgnore] public int AddedBy { get; set; }
        [JsonIgnore] public DateTime AddedOn { get; set; }
        [JsonIgnore] public int LastModBy { get; set; }
        [JsonIgnore] public DateTime LastModOn { get; set; }
    }

    public class CardResponse
    {
        public int Id { get; set; }
        public string CardNumber { get; set; }
    }
}