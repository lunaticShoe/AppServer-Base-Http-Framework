using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace AppServerBase.Utils
{
    public static class HashMethods
    {
        public static string SHA256Hash(string value)
        {
            using (SHA256 hash = SHA256.Create())
            {
                return string.Concat(hash
                  .ComputeHash(Encoding.UTF8.GetBytes(value))
                  .Select(item => item.ToString("x2")));
            }
        }

        public static string MD5Hash(string value)
        {
            byte[] hash = Encoding.ASCII.GetBytes(value);
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] hashenc = md5.ComputeHash(hash);
            string result = "";

            foreach (var b in hashenc)            
                result += b.ToString("x2");            

            return result;
        }

        public static string GetSign(string data)
        {
            var part1 = data.Substring(0, data.Length / 2);
            var part2 = data.Substring(data.Length / 2 + 1);

            part1 = SHA256Hash(MD5Hash(part1.ToUpper() + part2.ToUpper()));
            part2 = SHA256Hash(part2.ToLower() + MD5Hash(part2));

            return SHA256Hash(MD5Hash(part1.ToUpper()) + part2);
        }

        

    }
}
