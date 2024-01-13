using System.Security.Cryptography;
using System.Text;

namespace hoa7mlishe.Helpers
{
    internal static class CryptographyHelper
    {
        /// <summary>
        /// Хэширует строку в MD5
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        internal static byte[] HashString(string s)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(s);
            return MD5.HashData(bytes);
        }

    }
}
