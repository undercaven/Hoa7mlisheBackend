namespace hoa7mlishe.Trading.Models
{
    public class TradeDto
    {
        /// <summary>
        /// ID предложения
        /// </summary>
        public Guid OfferId { get; set; }

        /// <summary>
        /// Отправитель
        /// </summary>
        public Guid SenderId { get; set; }

        /// <summary>
        /// Получатель
        /// </summary>
        public Guid ReceiverId { get; set; }

        /// <summary>
        /// Предложение
        /// </summary>
        public List<TradeContentDto> Offer { get; set; }

        public List<TradeContentDto> Request { get; set; }

        public int Mikoins { get; set; }

        /// <summary>
        /// Дата отправки
        /// </summary>
        public DateTime SendTime { get; set; }
    }
}
