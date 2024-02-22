using AutoMapper;
using hoa7mlishe.API.Services.Interfaces;
using hoa7mlishe.API.Database.Context;
using hoa7mlishe.Database.Models;
using hoa7mlishe.Hoa7Enums;
using System.Security.Cryptography;
using hoa7mlishe.API.DTO.Cards;
using hoa7mlishe.API.Database.Models;
using Microsoft.EntityFrameworkCore;
using Windows.ApplicationModel;
using Microsoft.Extensions.FileSystemGlobbing.Internal.PatternContexts;
using Azure.Identity;

namespace hoa7mlishe.Services
{
    public class CardsService : ICardsService
    {
        private Hoa7mlisheContext _context;
        private IUserRequestService _userRequestService;
        public CardsService(Hoa7mlisheContext context, IUserRequestService userRequestService)
        {
            _context = context;
            _userRequestService = userRequestService;
        }

        private static IMapper PackDtoMapper => new MapperConfiguration(cfg => cfg.CreateMap<CardPack, CardPackDTO>()).CreateMapper();

        /// <summary>
        /// Получает все паки карточек, видимые пользователю
        /// </summary>
        /// <param name="userRole">Роль пользователя</param>
        /// <returns>Коллекция моделей паков</returns>
        public List<CardPackDTO> GetPacks(string userRole = "user")
        {
            List<CardPack> cardPacks = [];

            cardPacks = userRole != "admin" ? [.. _context.CardPacks.Where(x => !x.Hidden)] : [.. _context.CardPacks];

            var result = new List<CardPackDTO>();

            foreach (var pack in cardPacks)
            {
                CardPackDTO packDto = PackDtoMapper.Map<CardPackDTO>(pack);
                packDto.CardDistrib = [];
                for (int i = 1; i <= 6; ++i)
                {
                    packDto.CardDistrib.Add(_context.FileInfos.Count(x => x.Tag == pack.Tag && x.Rarity == i));
                }

                string[] raritiesStr = pack.CardDistribution.Split(';', StringSplitOptions.RemoveEmptyEntries);
                raritiesStr = [.. raritiesStr, "1000"];

                packDto.Rarities = new double[raritiesStr.Length];

                packDto.Rarities[0] = double.Parse(raritiesStr[0]) / 10;

                for (int i = 1; i < raritiesStr.Length; i++)
                {
                    double chanse = (double.Parse(raritiesStr[i]) - double.Parse(raritiesStr[i - 1])) / 10;
                    packDto.Rarities[i] = Math.Round(chanse, 1);
                }

                result.Add(packDto);
            }

            return [.. result.OrderBy(x => x.Price).ThenBy(x => x.Description)];
        }

        public void RemoveCard(User user, Guid cardId, bool shiny, int count)
        {
            var card = _context.CollectedCards.SingleOrDefault(x => x.UserId == user.Id && x.CardId == cardId && x.IsShiny == shiny);

            if (card?.Count - count <= 0)
            {
                _context.CollectedCards.Remove(card);
                return;
            }

            card.Count -= count;
            _context.Update(card);
            _context.SaveChanges();
        }

        public DetailedCardPackDTO GetPackInfoForUser(User user, Guid packID)
        {
            var pack = _context.CardPacks.Single(x => x.Id == packID);

            var packDto = new DetailedCardPackDTO()
            {
                Id = pack.Id,
                Description = pack.Description,
                Price = pack.Price,
                Tag = pack.Tag,
                CardCount = pack.CardCount,
                SeasonId = pack.SeasonId,
                Hidden = pack.Hidden,
                PreviewHash = pack.PreviewHash,
                ShinyCollected = new List<int>(),
                NormalCollected = new List<int>(),
                CoverImageId = pack.CoverImageId
            };

            for (int rarity = 1; rarity <= 6; rarity++)
            {
                packDto.ShinyCollected.Add(_context.CollectedCards.Count(x => x.UserId == user.Id && x.Card.Rarity == rarity && x.Card.Tag == pack.Tag && x.IsShiny == true));
                packDto.NormalCollected.Add(_context.CollectedCards.Count(x => x.UserId == user.Id && x.Card.Rarity == rarity && x.Card.Tag == pack.Tag && x.IsShiny == false));
            }

            packDto.CardDistrib = [];
            for (int i = 1; i <= 6; ++i)
            {
                packDto.CardDistrib.Add(_context.FileInfos.Count(x => x.Tag == pack.Tag && x.Rarity == i));
            }

            string[] raritiesStr = pack.CardDistribution.Split(';', StringSplitOptions.RemoveEmptyEntries);
            raritiesStr = [.. raritiesStr, "1000"];

            packDto.Rarities = new double[raritiesStr.Length];

            packDto.Rarities[0] = double.Parse(raritiesStr[0]) / 10;

            for (int i = 1; i < raritiesStr.Length; i++)
            {
                double chanse = (double.Parse(raritiesStr[i]) - double.Parse(raritiesStr[i - 1])) / 10;
                packDto.Rarities[i] = Math.Round(chanse, 1);
            }

            return packDto;
        }

        /// <summary>
        /// "Открывает" пак карточек, генерируя случайный набор карточек
        /// </summary>
        /// <param name="seasonId">Идентификатор сезона</param>
        /// <param name="packSize">Размер пака</param>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <returns>Коллекция карточек</returns>
        public List<CardInfoDTO> GenerateCardPack(int seasonId, int packSize, Guid userId)
        {
            var result = new List<CardInfoDTO>();

            IQueryable<CardInfo> seasonCards;
            if (seasonId != 0)
            {
                seasonCards = _context.FileInfos.Where(x => x.SeasonId == seasonId);
            }
            else
            {
                seasonCards = _context.FileInfos;
            }

            for (int i = 0; i < packSize; i++)
            {
                int rarity = GetRarity();
                List<CardInfo> suitableFiles = [.. seasonCards.Where(x => x.Rarity == rarity)];

                int r = RandomNumberGenerator.GetInt32(suitableFiles.Count);

                CardInfo card = suitableFiles[r];

                card.IsShiny = RandomNumberGenerator.GetInt32(10) == 9;

                result.Add(card.GetModel());
                CardCollected(userId, card);
            }

            return result;
        }

        public Dictionary<string, int> GetCardDistribution(int season)
        {
            var result = new Dictionary<string, int>();
            string[] names = [
                "Gray",
                "Green",
                "Blue",
                "Purple",
                "Golden"
            ];

            for (int i = 1; i <= 5; i++)
            {
                int probability = _context.FileInfos.Count(x => x.SeasonId == season && x.Rarity == i);

                result.Add(names[i - 1], probability);
            }

            return result;
        }

        /// <summary>
        /// "Открывает" пак карточек, генерируя случайный набор карточек
        /// </summary>
        /// <param name="packId">Идентификатор пака</param>
        /// <param name="user">Пользователь</param>
        /// <param name="packSize">Размер пака</param>
        /// <returns></returns>
        public List<CardInfoDTO> GenerateCardPack(Guid packId, User user, int packSize = 0)
        {
            CardPack pack = _context.CardPacks.FirstOrDefault(x => x.Id == packId);

            if (pack is null) return null;

            _userRequestService.UpdateMikoins(user, pack.Price, MikoinUpdateOptions.Subtractive);

            var result = new List<CardInfoDTO>();

            IQueryable<CardInfo> seasonCards = true switch
            {
                var _ when pack.SeasonId != 0 => _context.FileInfos.Where(x => x.SeasonId == pack.SeasonId),
                var _ when pack.Tag is not null => _context.FileInfos.Where(x => x.Tag == pack.Tag),
                _ => _context.FileInfos,
            };

            int cardCount = packSize == 0 ? pack.CardCount : packSize;

            for (int i = 0; i < cardCount; i++)
            {
                int rarity = GetRarity(pack.CardDistribution);
                List<CardInfo> suitableFiles = [.. seasonCards.Where(x => x.Rarity == rarity)];
                int r = RandomNumberGenerator.GetInt32(suitableFiles.Count);

                CardInfo card = suitableFiles[r].Clone();

                card.IsShiny = RandomNumberGenerator.GetInt32(9) == 8;

                result.Add(card.GetModel());
                CardCollected(user.Id, card);
            }

            return result;
        }

        /// <summary>
        /// Получает количество карточек, собранных пользователем
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public int GetCardCount(Guid userId, Guid? packId)
        {
            var user = _context.Users.First(x => x.Id == userId);

            if (packId is null)
            {
                return _context.CollectedCards.Count(x => x.UserId == userId);
            }

            var pack = _context.CardPacks.First(x => x.Id == packId);
            return _context.CollectedCards.Count(x => x.UserId == userId && x.Card.Tag == pack.Tag);
        }

        public List<CardInfo> GetAllCards()
        {
            return _context.FileInfos.ToList();
        }

        public int GetRarity()
        {
            int i = RandomNumberGenerator.GetInt32(0, 1001);

            return i switch
            {
                var _ when i < 500 => 1,
                var _ when i < 800 => 2,
                var _ when i < 920 => 3,
                var _ when i < 995 => 4,
                _ => 5,
            };
        }

        public bool GetShiny() => RandomNumberGenerator.GetInt32(9) == 8;

        public int GetRarity(string distribution)
        {
            int i = RandomNumberGenerator.GetInt32(0, 1001);

            string[] chances = distribution.Split(';');

            return i switch
            {
                var _ when i < double.Parse(chances[0]) => 1,
                var _ when i < double.Parse(chances[1]) => 2,
                var _ when i < double.Parse(chances[2]) => 3,
                var _ when i < double.Parse(chances[3]) => 4,
                _ => 5,
            };
        }

        public List<CollectedCardsDTO> GetPageOfCards(Guid userId, CardPageDto cardPage)
        {
            List<CollectedCard> cards =
                _context.CollectedCards.Where(x => x.UserId == userId)
                .Include(x => x.Card)
                .Include(x => x.User)
                .ToList();

            var cardsDto = new List<CollectedCardsDTO>();

            foreach (var card in cards)
            {
                bool isShiny = card.IsShiny;

                var cardDto = new CollectedCardsDTO()
                {
                    Count = card.Count,
                    CardInfo = card.Card.GetModel(),
                    IsShiny = isShiny
                };

                cardDto.CardInfo.IsShiny = isShiny;
                cardsDto.Add(cardDto);
            }

            if (cardPage.PackId is not null)
            {
                var pack = _context.CardPacks.First(x => x.Id == cardPage.PackId);
                cardsDto = cardsDto.Where(x => x.CardInfo.Tag == pack.Tag).ToList();
            }

            switch (cardPage.SortOrder)
            {
                case 0:
                    cardsDto = cardsDto.OrderBy(x => x.CardInfo.Rarity + Convert.ToDouble(x.IsShiny) * 0.5).ToList();
                    break;
                case 1:
                    cardsDto = cardsDto.OrderByDescending(x => x.CardInfo.Rarity + Convert.ToDouble(x.IsShiny) * 0.5).ToList();
                    break;
                case 2:
                    cardsDto = cardsDto.OrderBy(x => x.Count).ToList();
                    break;
                case 3:
                    cardsDto = cardsDto.OrderByDescending(x => x.Count).ToList();
                    break;
            }

            int arrayStart = (cardPage.Page - 1) * cardPage.PageSize;
            int offset;

            if (arrayStart + cardPage.PageSize < cardsDto.Count)
            {
                offset = cardPage.PageSize;
            }
            else
            {
                offset = cardsDto.Count - arrayStart;
            }

            return cardsDto.GetRange(arrayStart, offset);
        }

        public void CardCollected(Guid userId, CardInfo card, int count = 1)
        {
            var collectedRecord = _context.CollectedCards.Where(
                x => x.UserId == userId
                && x.CardId == card.Id
                && x.IsShiny == card.IsShiny
            ).FirstOrDefault();

            if (collectedRecord is not null)
            {
                collectedRecord.Count += count;
                _context.SaveChanges();
                return;
            }

            collectedRecord = new CollectedCard()
            {
                Id = Guid.NewGuid(),
                CardId = card.Id,
                UserId = userId,
                Count = count,
                IsShiny = card.IsShiny
            };

            _context.CollectedCards.Add(collectedRecord);
            _context.SaveChanges();
        }

        public List<string> GetAllTags()
        {
            return _context.FileInfos.Select(x => x.Tag).Distinct().ToList();
        }

        public CardInfoDTO CreateUltimate(User user, Guid packId, int count, bool shiny)
        {
            var pack = _context.CardPacks.First(x => x.Id == packId);

            var userCollectedCards = _context.CollectedCards.Where(x => x.UserId == user.Id && x.Card.Tag == pack.Tag && x.IsShiny == shiny && x.Card.Rarity <= 5).ToList();

            if (userCollectedCards.Any(u => u.Count < count))
            {
                throw new InvalidOperationException("POOPIE");
            }

            var cardsInPack = _context.FileInfos.Where(x => x.Tag == pack.Tag && x.Rarity <= 5).ToList();
           
            if (cardsInPack.Any(c => !userCollectedCards.Any(u => u.CardId == c.Id)))
            {
                throw new InvalidOperationException("POOPIE");
            }

            foreach (var card in userCollectedCards)
            {
                RemoveCard(user, card.CardId, shiny, count);
            }

            var newCard = _context.FileInfos.Single(x => x.Tag == pack.Tag && x.Rarity == 6);
            newCard.IsShiny = shiny;
            CardCollected(user.Id, newCard);

            return newCard.GetModel();
        }
    }
}
