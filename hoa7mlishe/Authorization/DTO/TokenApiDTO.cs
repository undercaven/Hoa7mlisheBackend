namespace hoa7mlishe.API.Authorization.DTO
{
    /// <summary>
    /// Модель токена для авторизации
    /// </summary>
    public class TokenApiDTO
    {
        /// <summary>
        /// Access Token
        /// </summary>
        public string? AccessToken { get; set; }

        /// <summary>
        /// Refresh Token
        /// </summary>
        public string? RefreshToken { get; set; }
    }
}
