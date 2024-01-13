using hoa7mlishe.API.Database.Models;
using hoa7mlishe.Trading.Models;

namespace hoa7mlishe.API.Trading.Services
{
    public interface ITradesService
    {
        public List<TradeDto> GetUserTrades(Guid userId, string status);

        public bool TransferCard(Guid cardId, User sender, User receiver, int count, bool shiny = false);

        public bool AcceptOffer(Guid offerId, User user);

        public Guid PostOffer(TradeOfferDto offer);
    }
}
