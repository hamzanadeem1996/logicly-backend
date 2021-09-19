using Apex.DataAccess.Models;
using NPoco;
using System;

namespace Apex.DataAccess.Repositories
{
    public class CardRepo
    {
        public CardResponse Get(int agencyId)
        {
            var cmd = Sql.Builder.Select($@"Id,CardNumber").From("Cards");
            cmd.Where("AgencyId=@0", agencyId);
            using (var db = Utility.Database)
            {
                var result = db.FirstOrDefault<CardResponse>(cmd) ?? new CardResponse();
                return result;
            }
        }

        public object Save(Card card, int currentLoggedUser)
        {
            using (var db = Utility.Database)
            {
                if (card.Id == 0)
                {
                    card.AddedOn = DateTime.UtcNow;
                    card.AddedBy = currentLoggedUser;
                }
                card.LastModOn = DateTime.UtcNow;
                card.LastModBy = currentLoggedUser;
                db.Save(card);
                return card;
            }
        }

        public int Delete(int Id)
        {
            using (var db = Utility.Database)
            {
                var result = db.Delete<Card>(Id);
                return Convert.ToInt32(result);
            }
        }
    }
}