using Apex.DataAccess;
using ElmahCore;
using StackExchange.Profiling.Internal;
using Stripe;
using System;
using System.Net;
using static Apex_Api.Common;

namespace Apex_Api.Service
{
    public class CardService
    {
        public object Save(Apex.DataAccess.Models.Card card, int agencyId, int currentLoggedUser)
        {
            var getAgency = Common.Instances.AgencyInst.GetAgency(agencyId);

            if (getAgency.Id == 0)
                throw new HttpException((int)HttpStatusCode.NotFound,
                    $" This agency {Utility.ResponseMessage.NotFound}");

            if (string.IsNullOrWhiteSpace(card.CardType))
            {
                card.CardType = Utility.FindType(card.CardNumber).ToString();
            }

            try
            {
                var cardResponse = new StripeService().AddCard(ref card, ref getAgency);
                getAgency.StripePaymentMethodId = cardResponse.Id;  // update StripePaymentMethodId
                Common.Instances.AgencyInst.SaveAgency(getAgency);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                throw new HttpException((int)HttpStatusCode.Unauthorized, ex.Message);
            }

            card.AgencyId = getAgency.Id;
            card.CardHolderName = Encryption.Encrypt(card.CardHolderName);
            card.CardNumber = Encryption.Encrypt(card.CardNumber);
            card.CardType = Encryption.Encrypt(card.CardType);
            card.ExpiryMonth = Encryption.Encrypt(card.ExpiryMonth);
            card.ExpiryYear = Encryption.Encrypt(card.ExpiryYear);
            card.Cvv = Encryption.Encrypt(card.Cvv);

            var GetCard = Common.Instances.CardInst.Get(card.AgencyId);
            if (GetCard.Id > 0)
                card.Id = GetCard.Id;

            Common.Instances.CardInst.Save(card, currentLoggedUser);

            if (getAgency.Id > 0 && !getAgency.StripeCustomerId.IsNullOrWhiteSpace())
            {
                var stripeService = new StripeService();
                var service = new SubscriptionService();
                var customer = new StripeService().GetCustomer(getAgency.StripeCustomerId);

                var options = new SubscriptionListOptions
                { Customer = customer.Id, };
                var subscriptions = service.List(options);

                if (subscriptions.Data.Count > 0)
                {
                    foreach (var sub in subscriptions.Data)
                    {
                        // UPDATE SUBSCRIPTION
                        if (sub != null && sub.Items.Data[0].Price.Id != getAgency.StripePriceId)
                            stripeService.UpdateSubscription(ref getAgency, sub.Id);
                    }
                }
                else
                {
                    // ADD SUBSCRIPTION
                    stripeService.CreateSubscription(ref getAgency);
                }
            }
            return card;
        }
    }
}