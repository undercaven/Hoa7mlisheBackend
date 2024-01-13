using hoa7mlishe.API.Database.Context;
using hoa7mlishe.API.Database.Models;
using hoa7mlishe.API.Services.Interfaces;
using hoa7mlishe.API.Trading.Services;
using hoa7mlishe.Trading.Models;

namespace hoa7mlishe.Trading.Services
{
    public class TradesService : ITradesService
    {
        private Hoa7mlisheContext _context;
        private IUserRequestService _userRequestService;
        private ICardsService _cardsService;

        public TradesService(IUserRequestService userRequestService, Hoa7mlisheContext context, ICardsService cardsService) 
        { 
            _context = context;
            _userRequestService = userRequestService;
            _cardsService = cardsService;
        }

        public List<TradeDto> GetUserTrades(Guid userId, string status)
        {
            List<TradeOffer> trades = _context.TradeOffers.Where(x => (x.SenderId == userId || x.ReceiverId == userId) && x.Status == status).ToList();

            var result = new List<TradeDto>();
            foreach(var trade in trades)
            {
                var tradeDto = new TradeDto()
                {
                    OfferId = trade.Id,
                    SenderId = trade.SenderId,
                    ReceiverId = trade.ReceiverId,
                    SendTime = trade.SendTime,
                    Mikoins = trade.Mikoins,
                    Offer = new List<TradeContentDto>(),
                    Request = new List<TradeContentDto>(),
                };

                var tradeContents = _context.TradeContents.Where(x => x.TradeId == trade.Id).ToList();

                foreach (var content in tradeContents)
                {
                    var contentDto = new TradeContentDto()
                    {
                        Card = _context.FileInfos.Where(x => x.Id == content.CardId).First(),
                        Count = content.Count,
                        IsShiny = content.IsShiny,
                    };

                    if(content.Request)
                    {
                        tradeDto.Request.Add(contentDto);
                    }
                    else
                    {
                        tradeDto.Offer.Add(contentDto);
                    }
                }

                result.Add(tradeDto);
            }

            return result;
        }

        public bool TransferCard(Guid cardId, User sender, User receiver, int count, bool shiny = false)
        {
            if (sender.Role == "admin")
            {
                var card = _context.FileInfos.Single(x => x.Id == cardId);
                card.IsShiny = shiny;
                _cardsService.CardCollected(receiver.Id, card, count);
                _context.SaveChanges();
                return true;
            }

            var senderCard = _context.CollectedCards.FirstOrDefault(x => x.UserId == sender.Id);
            if (senderCard is null)
            {
                return false;
            }

            if (count > senderCard.Count)
            {
                return false;
            }    

            var senderCardTracked = _context.CollectedCards.Update(senderCard);
            var receiverCard = _context.CollectedCards.FirstOrDefault(y => y.UserId == receiver.Id);
            if (receiverCard is null)
            {
                var card = _context.FileInfos.First(c => c.Id == cardId);
                _cardsService.CardCollected(receiver.Id, card, count);
                senderCardTracked.Entity.Count -= count;

                if (senderCardTracked.Entity.Count <= 0)
                {
                    _context.CollectedCards.Remove(senderCard);
                }

                _context.SaveChanges();
                return true;
            }

            senderCardTracked.Entity.Count -= count;

            if (senderCardTracked.Entity.Count <= 0)
            {
                _context.CollectedCards.Remove(senderCard);
            }

            var receiverCardTracked = _context.CollectedCards.Update(receiverCard);

            receiverCardTracked.Entity.Count += count;

            _context.SaveChanges();
            return true;
        }

        public bool AcceptOffer(Guid offerId, User user)
        {
            var trade = _context.TradeOffers.Where(x =>  offerId == x.Id).FirstOrDefault();

            if (trade?.Status != "pending" || trade?.SenderId == user.Id) { return false; }

            var secondUser = _userRequestService.GetUser(trade.SenderId);
            user.Mikoins += trade.Mikoins;
            trade.ConfirmationTime = DateTime.Now;
            var tradeContents = _context.TradeContents.Where(x => x.TradeId == offerId).ToList();

            foreach (var content in tradeContents)
            {
                CollectedCard senderCard, receiverCard;
                Guid senderId, receiverId;
                if (content.Request)
                {
                    senderCard = _context.CollectedCards.Where(x => x.UserId == user.Id && x.CardId == content.CardId).First();
                    receiverCard = _context.CollectedCards.Where(x => x.UserId == secondUser.Id && x.CardId == content.CardId).FirstOrDefault();
                    senderId = user.Id;
                    receiverId = secondUser.Id;
                }
                else
                {
                    senderCard = _context.CollectedCards.Where(x => x.UserId == secondUser.Id && x.CardId == content.CardId).First();
                    receiverCard = _context.CollectedCards.Where(x => x.UserId == user.Id && x.CardId == content.CardId).FirstOrDefault();

                    senderId = secondUser.Id;
                    receiverId = user.Id;
                }

                bool recCardFound = receiverCard is not null;
                if (!recCardFound)
                {
                    CardInfo ci = _context.FileInfos.First(x => x.Id == content.CardId);
                    _cardsService.CardCollected(receiverId, ci);
                    receiverCard = _context.CollectedCards.Where(x => x.UserId == receiverId && x.CardId == content.CardId).First();
                }


                var senderCardTracked = _context.CollectedCards.Update(senderCard);
                var receiverCardTracked = _context.CollectedCards.Update(receiverCard);

                senderCardTracked.Entity.Count -= content.Count;
                receiverCardTracked.Entity.Count += content.Count;
                if (senderCardTracked.Entity.Count <= 0)
                {
                    _context.CollectedCards.Remove(senderCardTracked.Entity);
                }

                if (!recCardFound)
                {
                    receiverCardTracked.Entity.Count--;
                }
            }

            trade.Status = "accepted";
            _context.SaveChanges();
            return true;
        }

        public Guid PostOffer(TradeOfferDto offer)
        {
            var model = new TradeOffer()
            {
                Id = Guid.NewGuid(),
                ReceiverId = offer.ReceiverId,
                SenderId = offer.SenderId,
                SendTime = DateTime.Now,
                ConfirmationTime = null,
                Status = "pending",
                Mikoins = offer.Mikoins
            };

            var created = _context.TradeOffers.Add(model);

            foreach(var content in offer.SenderOffer)
            {
                var contentModel = new TradeContent()
                {
                    CardId = content.CardId,
                    Count = content.Count,
                    Request = false,
                    TradeId = created.Entity.Id,
                    IsShiny = content.IsShiny
                };

                _context.TradeContents.Add(contentModel);
            }

            foreach(var content in offer.SenderRequest)
            {
                var contentModel = new TradeContent()
                {
                    CardId = content.CardId,
                    Count = content.Count,
                    Request = true,
                    TradeId = created.Entity.Id,
                    IsShiny = content.IsShiny
                };

                _context.TradeContents.Add(contentModel);
            }

            _context.SaveChanges();

            return created.Entity.Id;
        }
    }
}
