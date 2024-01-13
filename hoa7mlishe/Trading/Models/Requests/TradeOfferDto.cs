using hoa7mlishe.Trading.Models.Requests;

namespace hoa7mlishe.Trading.Models
{
    public class TradeOfferDto
    {
        public Guid SenderId { get; set; }
        public Guid ReceiverId { get; set; }
        public List<TradeOfferContentDto> SenderOffer { get; set; }
        public List<TradeOfferContentDto> SenderRequest { get; set; }
        public int Mikoins { get; set; }
    }
}
