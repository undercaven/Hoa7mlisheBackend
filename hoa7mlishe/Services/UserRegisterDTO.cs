namespace hoa7mlishe.API.Services
{
    /// <summary>
    /// Данные для регистрации
    /// </summary>
    public class UserRegisterDTO
    {
        /// <summary>
        /// Пароль
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Логин
        /// </summary>
        public string Login { get; set; }

        /// <summary>
        /// Имя пользователя
        /// </summary>
        public string Username { get; set; }
    }
}
