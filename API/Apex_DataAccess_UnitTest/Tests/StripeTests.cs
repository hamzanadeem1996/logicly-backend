using Apex.DataAccess.Models;
using Apex_Api.Service;
using StackExchange.Profiling.Internal;
using Xunit;

namespace Apex_DataAccess_UnitTest.Test
{
    public class StripeTest
    {
        private StripeService svr;

        public StripeTest()
        {
            svr = new StripeService();
        }

        [Fact]
        public void StartSubscription()
        {
            var agency = new Agency
            {
                StripeCustomerId = "cus_JP3wWWM3vT6aF8",
                StripePriceId = "price_1INiArByZlA80ds8BNJSeD5A"
            };
            var res = svr.CreateSubscription(ref agency);
            Assert.NotNull(res);
            Assert.True(!res.Id.IsNullOrWhiteSpace());
        }
    }
}