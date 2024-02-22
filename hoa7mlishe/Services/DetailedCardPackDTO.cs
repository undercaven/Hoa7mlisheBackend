namespace hoa7mlishe.Services
{
    public class DetailedCardPackDTO
    {
        /// <summary>
        /// Идентификатор пака
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Распределение карт
        /// </summary>
        public List<int> CardDistrib { get; set; }

        /// <summary>
        /// Шансы выпадения карт каждой редкости
        /// </summary>
        public double[] Rarities { get; set; }

        /// <summary>
        /// Цена
        /// </summary>
        public int Price { get; set; }

        /// <summary>
        /// Описание
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Количество карт
        /// </summary>
        public int CardCount { get; set; }

        /// <summary>
        /// Идентификатор обложки
        /// </summary>
        public Guid CoverImageId { get; set; }

        /// <summary>
        /// Тег
        /// </summary>
        public string? Tag { get; set; }

        /// <summary>
        /// Хеш обложки
        /// </summary>
        public string PreviewHash { get; set; }

        /// <summary>
        /// Признак скрытого пака
        /// </summary>
        public bool Hidden { get; set; }

        /// <summary>
        /// Номер сезона
        /// </summary>
        public int SeasonId { get; set; }

        public List<int> ShinyCollected { get; set; }

        public List<int> NormalCollected { get; set; }
    }
}