namespace hoa7mlishe.Helpers
{
    internal static class AuthorizationHelper
    {
        /// <summary>
        /// Генерирует accesstoken
        /// </summary>
        /// <param name="id">id пользователя</param>
        /// <returns>токен</returns>
        internal static string GenerateToken(Guid id, int hoursOffset)
        {
            byte[] guid = id.ToByteArray();
            byte[] expTime = BitConverter.GetBytes(DateTime.Now.AddHours(hoursOffset).ToBinary());

            return Convert.ToBase64String(expTime.Concat(guid).ToArray());
        }

        /// <summary>
        /// Расшифровывает токен
        /// </summary>
        /// <param name="token">токен</param>
        /// <param name="hoursOffset">количество часов, которое токен действителен</param>
        /// <returns> ID пользователя</returns>
        internal static Guid DecypherToken(string token, int hoursOffset = 3)
        {
            byte[] data = Convert.FromBase64String(token);
            DateTime when = DateTime.FromBinary(BitConverter.ToInt64(data, 0));
            if (DateTime.Compare(when, DateTime.UtcNow.AddHours(-hoursOffset)) < 0)
            {
                return Guid.Empty;
            }

            var guid = new Guid(data.Skip(8).ToArray());

            return guid;
        }

        /// <summary>
        /// Расшифровывает токен
        /// </summary>
        /// <param name="token">токен</param>
        /// <param name="hoursOffset">количество часов, которое токен действителен</param>
        /// <returns> ID пользователя</returns>
        internal static Guid DecypherToken(string token, ref DateTime when, int hoursOffset = 3)
        {
            byte[] data = Convert.FromBase64String(token);
            when = DateTime.FromBinary(BitConverter.ToInt64(data, 0));
            if (DateTime.Compare(when, DateTime.Now.AddHours(-hoursOffset)) < 0)
            {
                return Guid.Empty;
            }

            var guid = new Guid(data.Skip(8).ToArray());

            return guid;
        }
    }
}
