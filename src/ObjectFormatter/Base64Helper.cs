using System;
using System.Text;

namespace ObjectFormatter
{
    internal static class Base64Helper
    {
        public static string ToBase64(this string plainText)
        {
            if(string.IsNullOrEmpty(plainText))
            {
                return null;
            }
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        public static string FromBase64(this string base64EncodedData)
        {
            if (string.IsNullOrEmpty(base64EncodedData))
            {
                return null;
            }
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}
