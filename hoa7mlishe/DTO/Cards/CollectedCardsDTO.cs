using hoa7mlishe.API.Database.Models;

namespace hoa7mlishe.API.DTO.Cards
{
    public class CollectedCardsDTO
    {
        public int Count { get; set; }
        public CardInfo CardInfo { get; set; }
        public bool IsShiny { get; set; }
    }
}
