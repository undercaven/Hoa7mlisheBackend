namespace hoa7mlishe.API.DTO.Cards
{
    public class CardDTO
    {
        public IFormFile File { get; set; }
        public string Description { get; set; }
        public string Tag { get; set; }
        public int Rarity { get; set; }
    }
}
