
namespace hoa7mlishe.API.Database.Models
{
    public class CardInfoDTO
    {
        public Guid Id { get; internal set; }
        public string Tag { get; internal set; }
        public string Description { get; internal set; }
        public int Width { get; internal set; }
        public int SeasonId { get; internal set; }
        public string? PreviewHash { get; internal set; }
        public int Rarity { get; internal set; }
        public int Height { get; internal set; }
        public Guid? FileId { get; internal set; }
        public Guid? LayerOne { get; internal set; }
        public bool IsShiny { get; internal set; }
    }
}