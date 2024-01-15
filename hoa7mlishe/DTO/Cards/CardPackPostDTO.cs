namespace hoa7mlishe.API.DTO.Cards
{
    /// <summary>
    /// Модель с данными для создания пака карточек
    /// </summary>
    public class CardPackPostDTO
    {
        /// <summary>
        /// Обложка пака
        /// </summary>
        public IFormFile CoverImage { get; set; }

        /// <summary>
        /// Описание пака
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Количество карт
        /// </summary>
        public int CardCount { get; set; }

        /// <summary>
        /// Номер сезона
        /// </summary>
        public int SeasonId { get; set; }

        /// <summary>
        /// Цена
        /// </summary>
        public int Price { get; set; }

        /// <summary>
        /// Тег
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// Распределение карт
        /// </summary>
        public string CardDistribution { get; set; }
    }
}
