using hoa7mlishe.API.Database.Models;
using hoa7mlishe.API.DTO.Cards;
using hoa7mlishe.Database.Models;
using hoa7mlishe.Services;

namespace hoa7mlishe.API.Services.Interfaces
{
    public interface ICardsService
    {
        public List<CardPackDTO> GetPacks(string userRole = "user");

        public List<CardInfoDTO> GenerateCardPack(int seasonId, int packSize, Guid userId);

        public Dictionary<string, int> GetCardDistribution(int season);

        public List<CardInfoDTO> GenerateCardPack(Guid packId, User user, int packSize = 0);

        public int GetCardCount(Guid userId, Guid? packId);

        public List<CardInfo> GetAllCards();

        public List<string> GetAllTags();

        public int GetRarity();

        public bool GetShiny();

        public int GetRarity(string distribution);

        public List<CollectedCardsDTO> GetPageOfCards(Guid userId, CardPageDto cardPage);

        public void CardCollected(Guid userId, CardInfo card, int count = 1);
        public DetailedCardPackDTO GetPackInfoForUser(User user, Guid pack);
        public void RemoveCard(User user, Guid cardId, bool shiny, int count);
        public CardInfoDTO CreateUltimate(User user, Guid packId, int count, bool shiny);
    }
}
