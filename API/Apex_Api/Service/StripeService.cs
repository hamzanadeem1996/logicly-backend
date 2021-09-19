using Apex.DataAccess.Models;
using Microsoft.Extensions.Configuration;
using Stripe;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Apex_Api.Service
{
    public class StripeService
    {
        private static IConfiguration config;

        public StripeService()
        {
            var root = Path.GetDirectoryName(Assembly.GetExecutingAssembly()?.Location);

            if (config == null)
                config = new ConfigurationBuilder()
                    .SetBasePath(root).AddJsonFile("appsettings.json").Build();

            var mode = config.GetSection("mode").Value;
            var STRIPESECRETKEY = mode == "prod"
                ? config.GetSection("StripeKeys").GetSection("LiveKey").Value
                : config.GetSection("StripeKeys").GetSection("TestKey").Value;
            StripeConfiguration.ApiKey = STRIPESECRETKEY;
        }

        /// <summary>
        /// Add card to Stripe
        /// </summary>
        /// <param name="card"></param>
        /// <returns></returns>
        public PaymentMethod AddCard(ref Apex.DataAccess.Models.Card card, ref Agency agency)
        {
            var options = new PaymentMethodCreateOptions
            {
                Type = "card",

                Card = new PaymentMethodCardOptions
                {
                    Number = card.CardNumber,
                    ExpMonth = Convert.ToInt64(card.ExpiryMonth),
                    ExpYear = Convert.ToInt64(card.ExpiryYear),
                    Cvc = Convert.ToString(card.Cvv),
                }
            };
            var service = new PaymentMethodService();
            var res = service.Create(options);

            var attachOptions = new PaymentMethodAttachOptions
            {
                Customer = agency.StripeCustomerId
            };
            service.Attach(
                res.Id,
                attachOptions
            );

            //SET AS DEFAULT CARD
            var customerService = new CustomerService();
            var cust = customerService.Update(agency.StripeCustomerId, options: new CustomerUpdateOptions
            {
                InvoiceSettings = new CustomerInvoiceSettingsOptions
                {
                    DefaultPaymentMethod = res.Id
                }
            });
            return res;
        }

        public Customer GetCustomer(string stripeCustomerId)
        {
            var service = new CustomerService();
            return service.Get(stripeCustomerId);
        }

        public StripeList<Customer> GetCustomerFromStripe(string email)
        {
            var options = new CustomerListOptions
            {
                Limit = 1,
                Email = email
            };
            var service = new CustomerService();
            var customers = service.List(
                options
            );
            return customers;
        }

        public bool CustomerExistsInStripe(string email)
        {
            return GetCustomerFromStripe(email).Data.Count > 0;
        }

        public string GetStripeCustomerId(string email)
        {
            return GetCustomerFromStripe(email).Data.FirstOrDefault()?.Id;
        }

        public Customer AddCustomerToStripe(ref Agency agency)
        {
            var existingCustomer = GetCustomerFromStripe(agency.Email);
            if (existingCustomer != null && existingCustomer.Data.Count > 0)
            {
                return existingCustomer.Data.FirstOrDefault();
            }

            var options = new CustomerCreateOptions
            {
                Description = agency.Name,
                Address = new AddressOptions
                {
                    City = agency.City,
                    Country = agency.Country,
                    State = agency.State,
                    PostalCode = agency.ZipCode
                },
                Email = agency.Email,
                Name = agency.Name,
            };
            var service = new CustomerService();
            var res = service.Create(options);

            return res;
        }

        private StripeList<PaymentMethod> GetCustomerCards(string stripeCustomerId)
        {
            var options = new PaymentMethodListOptions
            {
                Customer = stripeCustomerId,
                Type = "card",
            };
            var service = new PaymentMethodService();
            var paymentMethods = service.List(
                options
            );
            return paymentMethods;
        }

        // public void ChargeCustomer(long amount, Agency agency)
        // {
        //
        //     var stripeCard = new Stripe.PaymentMethod();
        //
        //     //GET CUSTOMER CARDS FROM STRIPE
        //     var cards = GetCustomerCards(agency.StripeCustomerId);
        //     if (cards != null && cards.Any())
        //         stripeCard = cards.Data.FirstOrDefault();
        //
        //     var options = new PaymentIntentCreateOptions
        //     {
        //         Amount = amount * 100,
        //         Currency = "usd",
        //         PaymentMethodTypes = new List<string>
        //         {
        //             "card",
        //         },
        //         Description = $"Test stripe",
        //         Metadata = new Dictionary<string, string>
        //         {
        //
        //             {"Client ID", "1234"},
        //             {"Client Name", "som test"},
        //             {"Client Email", "somya.nextpage@gmail.com"}
        //         },
        //         Customer = agency.StripeCustomerId,
        //         PaymentMethod = stripeCard.Id,
        //         Confirm = true,
        //         CaptureMethod = "automatic"
        //     };
        //
        //     var service = new PaymentIntentService();
        //     var intent = service.Create(options);
        //
        //
        //     // return charge;
        // }

        public Subscription CreateSubscription(ref Agency agency)
        {
            var options = new SubscriptionCreateOptions
            {
                Customer = agency.StripeCustomerId,
                Items = new List<SubscriptionItemOptions>
                {
                    new SubscriptionItemOptions
                    {
                        Price = agency.StripePriceId
                    },
                },
                TrialEnd = DateTime.UtcNow.AddDays(15)
            };
            var service = new SubscriptionService();
            var res = service.Create(options);
            return res;
        }

        public object UpdateSubscription(ref Agency agency, string subscriptionId)
        {
            var options = new SubscriptionUpdateOptions
            {
                Items = new List<SubscriptionItemOptions>
                { new SubscriptionItemOptions{ Price = agency.StripePriceId },},
            };
            var service = new SubscriptionService();
            var res = service.Update(subscriptionId, options);
            return res;
        }
    }
}