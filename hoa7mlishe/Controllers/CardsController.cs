using hoa7mlishe.API.Services.Interfaces;
using hoa7mlishe.API.Database.Context;
using hoa7mlishe.Database.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using hoa7mlishe.API.Database.Models;

namespace hoa7mlishe.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("CorsPolicy")]
    public class CardsController : ControllerBase
    {
        private readonly Hoa7mlisheContext _context;
        private readonly ICardsService _cardsService;
        private IUserRequestService _userRequestService;
        private ILogger<CardsController> _logger;

        public CardsController(Hoa7mlisheContext context, ICardsService cardsSvc, IUserRequestService userRequestService, ILogger<CardsController> logger)
        {
            _context = context;
            _cardsService = cardsSvc;
            _userRequestService = userRequestService;
            _logger = logger;
        }

        /// <summary>
        /// Получает информацию о случайном файле
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="packId">ID пака</param>
        /// <returns>ИД и описание файла</returns>
        [HttpGet]
        public IActionResult GetRandomCard(
            [FromQuery] Guid packId,
            [FromHeader] string accessToken)
        {
            User user = _userRequestService.GetUser(accessToken);
            var card = _cardsService.GenerateCardPack(packId, user, 1).First();

            return Ok(card);
        }

        /// <summary>
        /// Возвращает информацию о всех паках карт
        /// </summary>
        /// <returns></returns>
        [HttpGet("packs")]
        public IActionResult GetCardPacks([FromHeader] string accessToken)
        {
            var user = _userRequestService.GetUser(accessToken);

            _logger.LogInformation(user.Username);

            return Ok(_cardsService.GetPacks(user.Role));
        }

        /// <summary>
        /// Возвращает информацию о конкретном паке карт
        /// </summary>
        /// <returns></returns>
        [HttpGet("packs/info/{packId}")]
        public IActionResult GetPackInfo(Guid packId) => Ok(_context.CardPacks.Single(x => x.Id == packId));

        /// <summary>
        /// Возвращает случайные карты для пака
        /// </summary>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        [HttpGet("packs/{packId}")]
        public IActionResult GetCardPack(
            Guid packId,
            [FromHeader] string accessToken)
        {
            User user = _userRequestService.GetUser(accessToken);

            if (user is null)
            {
                return Unauthorized();
            }

            try
            {
                List<CardInfo> cardPack = _cardsService.GenerateCardPack(packId, user);
                return Ok(cardPack);
            }
            catch
            {
                return Conflict();
            }
        }

        /// <summary>
        /// Возвращает инфо о дистрибуции карт в сезоне
        /// </summary>
        /// <param name="season">номер сезона</param>
        /// <returns></returns>
        [HttpGet("stats/{season}")]
        public IActionResult GetCardsDistribution(int season) => Ok(_cardsService.GetCardDistribution(season));

        /// <summary>
        /// Возвращает карточки полученные текущим пользователем
        /// </summary>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        [HttpGet("collected/{userId}/count/")]
        public IActionResult GetCollectedCards(Guid userId) => Ok(_cardsService.GetCardCount(userId));

        /// <summary>
        /// Возвращает карточки на n-ой странице
        /// </summary>
        /// <param name="page">номер страницы</param>
        /// <param name="pageSize">количество карт на странице</param>
        /// <param name="season">номер сезона</param>
        /// <param name="sortOrder">0 - редкость ASC, 1 - редкость DESC, 2 - количество ASC, 3 - количество DESC</param>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        [HttpGet("collected/{userId}/{page}")]
        public IActionResult GetCardsPage(
            Guid userId,
            int page,
            int pageSize,
            int sortOrder,
            int season)
        {
            var cardPage = new CardPageDto()
            {
                Page = page,
                PageSize = pageSize,
                SortOrder = sortOrder,
                Season = season
            };
            
            var response = _cardsService.GetPageOfCards(userId, cardPage);
            return Ok(response);
        }

        /// <summary>
        /// Возвращает все карточки в базе
        /// </summary>
        /// <returns>Массив информации о фото</returns>
        [HttpGet("gallery")]
        public IActionResult GetAllCards() => Ok(_cardsService.GetAllCards());

        [HttpGet("tags")]
        public IActionResult GetAllTags() => Ok(_cardsService.GetAllTags());
    }
}
