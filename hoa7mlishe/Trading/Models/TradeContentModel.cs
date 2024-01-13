using Microsoft.EntityFrameworkCore;

namespace hoa7mlishe.Trading.Models
{
    [PrimaryKey("TradeId","CardId")]
    public class TradeContentModel
    {
        public Guid TradeId { get; set; }
        public Guid CardId { get; set; }
        public bool Request { get; set; }
        public int Count { get; set; }
        public bool IsShiny { get; set; }
    }
}
