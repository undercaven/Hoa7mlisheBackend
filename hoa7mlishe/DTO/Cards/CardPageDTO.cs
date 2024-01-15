namespace hoa7mlishe.Database.Models
{
    /// <summary>
    /// Модель страницы с картами
    /// </summary>
    public class CardPageDto
    {
        /// <summary>
        /// Номер страницы
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Размер страницы
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 0 - редкость ASC, 1 - редкость DESC, 2 - количество ASC, 3 - количество DESC
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// Номер сезона
        /// </summary>
        public int Season { get; set; }
    }
}
