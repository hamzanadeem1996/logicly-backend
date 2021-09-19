using Apex.DataAccess;
using Xunit;

namespace Apex_DataAccess_UnitTest.Test
{
    public class EncryptionTest
    {
        [Fact]
        public void EncryptAndDecrypt()
        {
            //encrypt                             //Decrypt
            //18B08B08BCB32593B6D40654C3A3E330    Curran

            var result = "18B08B08BCB32593B6D40654C3A3E330";
            const string val = "Curran";

            var enc1 = Encryption.Encrypt(val);
            Assert.True(enc1 == result);
            var unen2 = Encryption.Decrypt(enc1);
            Assert.True(unen2 == val);
        }
    }
}