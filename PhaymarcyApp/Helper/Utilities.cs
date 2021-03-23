using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PhaymarcyApp.Helper
{
    static class Utilities
    {
        public static bool IsEmpty(params string[] array)
        {
            foreach (var item in array)
            {
                if (string.IsNullOrWhiteSpace(item))
                {
                    return false;
                }
            }
            return true;
        }
        public static string HashCode(this string password)
        {
            byte[] getByte = new ASCIIEncoding().GetBytes(password);
            byte[] hashByte = new SHA256Managed().ComputeHash(getByte);
            return new ASCIIEncoding().GetString(hashByte);
        }
    }
}
