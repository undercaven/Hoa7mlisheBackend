using hoa7mlishe.API.Authorization.DTO;

namespace hoa7mlishe.API.DTO.Users
{
    /// <summary>
    /// Модель пользователя
    /// </summary>
    public class UserResponseDTO
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Токены
        /// </summary>
        public TokenApiDTO Tokens { get; set; }

        /// <summary>
        /// Роль
        /// </summary>
        public string Role { get; set; }

        /// <summary>
        /// Никнейм
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Количество микоинов
        /// </summary>
        public long Mikoins { get; set; }

        /// <summary>
        /// Идентификатор аватара
        /// </summary>
        public Guid? AvatarId { get; set; }
    }
}
