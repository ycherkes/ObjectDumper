using System;
using System.Text;

namespace ObjectDumper
{
    internal static class Base64Helper
    {
        public static (bool, string) Base64Decode(this string base64EncodedData)
        {
            try
            {
                var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
                return (true, Encoding.UTF8.GetString(base64EncodedBytes));
            }
            catch
            {
                return (false, base64EncodedData);
            }
        }
    }
}
