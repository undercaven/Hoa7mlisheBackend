namespace hoa7mlishe.Trading.Models
{
    public class TradeModel
    {
        public Guid Id { get; set; }
        public Guid SenderId { get; set; }
        public Guid ReceiverId { get; set; }
        public DateTime SendTime { get; set; }
        public string Status { get; set; }
        public DateTime? ConfirmationTime { get; set; }
        public int Mikoins { get; set; }
    }
}
