using hoa7mlishe.API.Services.Interfaces;
using hoa7mlishe.API.Database.Context;
using hoa7mlishe.Database.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using hoa7mlishe.API.Database.Models;
using Microsoft.AspNetCore.Authorization;
using hoa7mlishe.API.Authorization.Helpers;
using System.Security.Claims;

namespace hoa7mlishe.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("CorsPolicy")]
    [Authorize]
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
        /// <param name="packId">ID пака</param>
        /// <returns>ИД и описание файла</returns>
        [HttpPost]
        public IActionResult GetRandomCard(
            [FromQuery] Guid packId)
        {
            User user = _userRequestService.GetUser(User.Identity);
            var card = _cardsService.GenerateCardPack(packId, user, 1).First();

            return Ok(card);
        }

        /// <summary>
        /// Возвращает информацию о всех паках карт
        /// </summary>
        /// <returns></returns>
        [HttpGet("packs")]
        public IActionResult GetCardPacks()
        {
            var user = _userRequestService.GetUser(User.Identity);

            return Ok(_cardsService.GetPacks(user.Role));
        }

        /// <summary>
        /// Создает ультимтивную карточку для пака, если у пользователя есть все карточки редкостей 1-5
        /// </summary>
        /// <param name="packId">id пака карточек</param>
        /// <param name="count">количество создаваемых карточек</param>
        /// <param name="shiny">индикатор блестящей карточки</param>
        /// <returns></returns>
        [HttpPost("createUltimate/{packId}")]
        public IActionResult CreateUltimate(Guid packId, int count, bool shiny)
        {
            User user = _userRequestService.GetUser(User.Identity);

            if (user is null) return Unauthorized();
            try
            {
                var cardId = _cardsService.CreateUltimate(user, packId, count, shiny);
                return Ok(cardId);

            }
            catch (InvalidDataException)
            {
                return Conflict();
            }
        }

            /// <summary>
            /// Возвращает информацию о конкретном паке карт
            /// </summary>
            /// <returns></returns>
            [HttpGet("packs/info/{packId}")]
        public IActionResult GetPackInfo(Guid packId)
        {
            User user = _userRequestService.GetUser(User.Identity);

            if (user is null)
            {
                return Unauthorized();
            }

            var result = _cardsService.GetPackInfoForUser(user, packId);

            return Ok(result);
        }

        /// <summary>
        /// Возвращает все карточки, содержащиеся в паке
        /// </summary>
        /// <param name="packId">id пака</param>
        /// <returns></returns>
        [HttpGet("packs/info/{packId}/getcards")]
        public IActionResult GetPackCards(Guid packId)
        {
            User user = _userRequestService.GetUser(User.Identity);
            if (user is null)
            {
                return Unauthorized();
            }

            var pack = _context.CardPacks.Single(x => x.Id == packId);
            return Ok(_context.FileInfos.Where(x => x.Tag == pack.Tag).OrderBy(x => x.Rarity).Select(x => x.GetModel()).ToList());
        }

        /// <summary>
        /// Открывает пак карточек
        /// </summary>
        /// <param name="packId">Идентификатор пака</param>
        /// <returns>Массив сгенерированных карточек</returns>
        [HttpPost("packs/{packId}")]
        public IActionResult GetCardPack(
            Guid packId)
        {
            User user = _userRequestService.GetUser(User.Identity);

            if (user is null)
            {
                return Unauthorized();
            }

            try
            {
                List<CardInfoDTO> cardPack = _cardsService.GenerateCardPack(packId, user);
                return Ok(cardPack);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while generating card pack");
                return Conflict();
            }
        }

        /// <summary>
        /// Возвращает инфо о дистрибуции карт в сезоне
        /// </summary>
        /// <param name="season">номер сезона</param>
        /// <returns></returns>
        [HttpGet("stats/{season}")]
        [AllowAnonymous]
        public IActionResult GetCardsDistribution(int season) => Ok(_cardsService.GetCardDistribution(season));

        /// <summary>
        /// Возвращает карточки полученные текущим пользователем
        /// </summary>
        /// <returns></returns>
        [HttpGet("collected/{userId}/count/")]
        [AllowAnonymous]
        public IActionResult GetCollectedCards(Guid userId, Guid? packId) => Ok(_cardsService.GetCardCount(userId, packId));

        /// <summary>
        /// Возвращает карточки на n-ой странице
        /// </summary>
        /// <param name="page">номер страницы</param>
        /// <param name="pageSize">количество карт на странице</param>
        /// <param name="season">номер сезона</param>
        /// <param name="sortOrder">0 - редкость ASC, 1 - редкость DESC, 2 - количество ASC, 3 - количество DESC</param>
        /// <returns></returns>
        [HttpGet("collected/{userId}/{page}")]
        [AllowAnonymous]
        public IActionResult GetCardsPage(
            Guid userId,
            int page,
            int pageSize,
            int sortOrder,
            Guid? packId)
        {
            var cardPage = new CardPageDto()
            {
                Page = page,
                PageSize = pageSize,
                SortOrder = sortOrder,
                PackId = packId
            };
            
            var response = _cardsService.GetPageOfCards(userId, cardPage);
            return Ok(response);
        }

        /// <summary>
        /// Возвращает все карточки в базе
        /// </summary>
        /// <returns>Массив информации о фото</returns>
        [AllowAnonymous]
        [HttpGet("gallery")]
        public IActionResult GetAllCards() => Ok(_cardsService.GetAllCards());

        /// <summary>
        /// Получает список всех тегов карточек
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("tags")]
        public IActionResult GetAllTags() => Ok(_cardsService.GetAllTags());
    }
}
