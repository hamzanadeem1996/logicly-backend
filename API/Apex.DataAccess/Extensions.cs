using Apex_Api;

namespace Apex.DataAccess
{
    public static class Extensions
    {
        public static string Encrypt(this string val)
        {
            return Encryption.Encrypt(val);
        }

        public static string Decrypt(this string val)
        {
            return Encryption.Decrypt(val);
        }
        public static bool IsWeekend(this string val)
        {
            return val == Constant.SATURDAY || val == Constant.SUNDAY;
        }
    }
}