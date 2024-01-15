using hoa7mlishe.API.Database.Context;
using hoa7mlishe.API.Database.Models;
using hoa7mlishe.API.Services.Interfaces;
using hoa7mlishe.API.Trading.Models;
using hoa7mlishe.API.Trading.Services;
using hoa7mlishe.Trading.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace hoa7mlishe.API.Controllers
{
    /// <summary>
    /// Контроллер для операций обмена между пользователями
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [EnableCors("CorsPolicy")]
    [Authorize]
    public class TradesController : ControllerBase
    {
        private Hoa7mlisheContext _context;
        private IUserRequestService _userRequestService;
        private ITradesService _tradesService;

        /// <summary>
        /// Конструктор класса
        /// </summary>
        /// <param name="context">Контекст БД</param>
        /// <param name="userRequestService">Сервис для работы с пользователями</param>
        /// <param name="tradesService">Сервис для работы с обменом</param>
        public TradesController(Hoa7mlisheContext context, IUserRequestService userRequestService, ITradesService tradesService)
        {
            _context = context;
            _userRequestService = userRequestService;
            _tradesService = tradesService;
        }

        /// <summary>
        /// Создает предложение обмена
        /// </summary>
        /// <param name="offer">Предложение обмена</param>
        /// <returns></returns>
        [HttpPost("offers")]
        public IActionResult PostTradeOffer(
            [FromBody] TradeOfferDto offer)
        {
            var sender = _userRequestService.GetUser(User.Identity);

            if (sender?.Id != offer.SenderId)
            {
                return Unauthorized();
            }

            var receiver = _userRequestService.GetUser(offer.ReceiverId);

            if (receiver is null)
            {
                return BadRequest();
            }

            _tradesService.PostOffer(offer);

            return Ok();
        }

        /// <summary>
        /// Принимает предложение обмена
        /// </summary>
        /// <param name="offerId">Идентификатор предложения</param>
        /// <returns></returns>
        [HttpPut("offers/{offerId}/confirm")]
        public IActionResult AcceptTradeOffer(
            Guid offerId)
        {
            User user = _userRequestService.GetUser(User.Identity);
            if (user is null)
            {
                return Unauthorized();
            }

            bool result = _tradesService.AcceptOffer(offerId, user);

            return result ? Ok() : BadRequest();
        }

        /// <summary>
        /// Получает предложения обмена для пользователя
        /// </summary>
        /// <param name="status">Статус предложений, которые требуется отобрать</param>
        /// <returns></returns>
        [HttpGet("offers/{status}")]
        public IActionResult GetTradeOffers(
            string status)
        {
            var user = _userRequestService.GetUser(User.Identity);

            if (user is null)
            {
                return Unauthorized();
            }

            var result = _tradesService.GetUserTrades(user.Id, status);

            return Ok(result);
        }

        /// <summary>
        /// Дарит карточку другому пользователю
        /// </summary>
        /// <param name="giftInfo">Описание подарка</param>
        /// <returns></returns>
        [HttpPut("sendGift")]
        public IActionResult GiftCard(
            [FromBody] GiftDto giftInfo)
        {
            var sender = _userRequestService.GetUser(User.Identity);
            if (sender is null)
            {
                return Unauthorized();
            }

            var receiver = _userRequestService.GetUser(giftInfo.ReceiverId);
            if (receiver is null)
            {
                return BadRequest("Receiver not found");
            }

            bool sendResult = _tradesService.TransferCard(giftInfo.CardId, sender, receiver, giftInfo.CardCount, giftInfo.IsShiny);

            return sendResult ? Ok() : BadRequest("Could not complete transaction");
        }
    }
}
