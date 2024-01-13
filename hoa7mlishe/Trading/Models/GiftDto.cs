namespace hoa7mlishe.API.Trading.Models
{
    public class GiftDto
    {
        public Guid ReceiverId { get; set; }
        public Guid CardId { get; set; }
        public int CardCount { get; set; }
        public bool IsShiny { get; set; }
    }
}
