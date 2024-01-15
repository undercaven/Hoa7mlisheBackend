namespace hoa7mlishe.API.DTO.Users
{
    /// <summary>
    /// Модель для авторизации пользователя
    /// </summary>
    public class UserLoginDTO
    {
        /// <summary>
        /// Логин
        /// </summary>
        public string Login { get; set; }

        /// <summary>
        /// Пароль
        /// </summary>
        public string Password { get; set; }
    }
}
