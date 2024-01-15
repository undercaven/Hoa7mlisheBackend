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
        public string Password { get; internal set; }

        /// <summary>
        /// Логин
        /// </summary>
        public string Login { get; internal set; }

        /// <summary>
        /// Имя пользователя
        /// </summary>
        public string Username { get; internal set; }
    }
}
