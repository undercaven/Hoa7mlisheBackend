using hoa7mlishe.API.Database.Models;

namespace hoa7mlishe.API.DTO.Cards
{
    /// <summary>
    /// Модель собранной карточки
    /// </summary>
    public class CollectedCardsDTO
    {
        /// <summary>
        /// Количество экземпляров карточки у пользователя
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Модель карточки
        /// </summary>
        public CardInfo CardInfo { get; set; }

        /// <summary>
        /// Признак блестящей карточки
        /// </summary>
        public bool IsShiny { get; set; }
    }
}
