using hoa7mlishe.API.Database.Models;
using hoa7mlishe.API.DTO.Users;

namespace hoa7mlishe.API.Database
{
    /// <summary>
    /// Методы расширений
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Возвращает уменьшенную модель пользователя
        /// </summary>
        /// <param name="user">пользователь</param>
        /// <returns></returns>
        public static UserShortDTO GetShortDto(this User user) => new()
        {
            Username = user.Username,
            Role = user.Role,
            Mikoins = user.Mikoins,
            Id = user.Id,
            AvatarId = user.AvatarId
        };
    }
}
