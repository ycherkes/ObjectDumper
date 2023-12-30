using System;
using System.Text;

namespace YellowFlavor.Serialization.Extensions
{
    internal static class Base64Extensions
    {
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
