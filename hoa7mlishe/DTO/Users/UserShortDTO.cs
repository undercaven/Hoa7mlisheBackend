namespace hoa7mlishe.API.DTO.Users
{
    /// <summary>
    /// Краткая модель пользователя
    /// </summary>
    public class UserShortDTO
    {
        /// <summary>
        /// Никнейм
        /// </summary>
        public string Username { get; set; }
        
        /// <summary>
        /// Роль
        /// </summary>
        public string Role { get; set; }

        /// <summary>
        /// Количество микоинов
        /// </summary>
        public long Mikoins { get; set; }

        /// <summary>
        /// Идентификатор пользователя
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Идентификатор изображения профиля
        /// </summary>
        public Guid? AvatarId { get; set; }
    }
}
