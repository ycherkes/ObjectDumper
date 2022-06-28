using System;
using System.Text;

namespace ObjectDumper.Extensions
{
    internal static class Base64Extensions
    {
        public static string ToBase64(this string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
            {
                return null;
            }
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        public static string Base64Decode(this string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}
