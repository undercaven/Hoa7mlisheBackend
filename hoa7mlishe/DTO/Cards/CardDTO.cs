namespace hoa7mlishe.API.DTO.Cards
{
    /// <summary>
    /// Карточка
    /// </summary>
    public class CardDTO
    {
        /// <summary>
        /// Изображение на карточке
        /// </summary>
        public IFormFile File { get; set; }

        /// <summary>
        /// Описание
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Тег
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// Редкость
        /// </summary>
        public int Rarity { get; set; }
    }
}
