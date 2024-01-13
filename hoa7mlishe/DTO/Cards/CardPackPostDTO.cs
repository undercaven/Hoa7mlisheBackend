namespace hoa7mlishe.API.DTO.Cards
{
    public class CardPackPostDTO
    {
        public IFormFile CoverImage { get; set; }
        public string Description { get; set; }
        public int CardCount { get; set; }
        public int SeasonId { get; set; }
        public int Price { get; set; }
        public string Tag { get; set; }
        public string CardDistribution { get; set; }
    }
}
