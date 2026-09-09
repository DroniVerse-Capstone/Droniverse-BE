using System.Security.Cryptography;
using System.Text;


namespace Droniverse.Community.Application.Services.Mongo
{
    public static class PaymentUtils
    {
        /// <summary>
        /// Computes HMAC-SHA256 hash of input data using the provided secret key.
        /// Returns a lowercase hexadecimal string representation of the hash.
        /// </summary>
        public static string ComputeHmacSha256(string data, string key)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }
}
