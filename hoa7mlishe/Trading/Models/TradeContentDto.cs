using hoa7mlishe.API.Database.Models;

namespace hoa7mlishe.Trading.Models
{
    public class TradeContentDto
    {
        public CardInfo Card { get; set; }
        public bool IsShiny { get; set; }
        public int Count { get; set; }
    }
}
