using hoa7mlishe.API.Database.Context;
using hoa7mlishe.API.Database.Models;
using hoa7mlishe.API.Services.Interfaces;
using hoa7mlishe.API.Trading.Models;
using hoa7mlishe.API.Trading.Services;
using hoa7mlishe.Trading.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace hoa7mlishe.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableCors("CorsPolicy")]
    public class TradesController : ControllerBase
    {
        private Hoa7mlisheContext _context;
        private IUserRequestService _userRequestService;
        private ITradesService _tradesService;

        public TradesController(Hoa7mlisheContext context, IUserRequestService userRequestService, ITradesService tradesService)
        {
            _context = context;
            _userRequestService = userRequestService;
            _tradesService = tradesService;
        }

            [HttpPost("offers")]
        public IActionResult PostTradeOffer(
            [FromBody] TradeOfferDto offer,
            [FromHeader] string accessToken)
        {
            var sender = _userRequestService.GetUser(accessToken);

            if (sender is null || sender?.Id != offer.SenderId)
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

        [HttpPut("offers/{offerId}/confirm")]
        public IActionResult AcceptTradeOffer(
            Guid offerId,
            [FromHeader] string accessToken)
        {
            User user = _userRequestService.GetUser(accessToken);
            if (user is null)
            {
                return Unauthorized();
            }

            bool result = _tradesService.AcceptOffer(offerId, user);

            return result ? Ok() : BadRequest();
        }

        [HttpGet("offers/{status}")]
        public IActionResult GetTradeOffers(
            string status,
            [FromHeader] string accessToken)
        {
            var user = _userRequestService.GetUser(accessToken);

            if (user is null)
            {
                return Unauthorized();
            }

            var result = _tradesService.GetUserTrades(user.Id, status);

            return Ok(result);
        }

        [HttpPut("sendGift")]
        public IActionResult GiftCard(
            [FromHeader] string accessToken,
            [FromBody] GiftDto giftInfo)
        {
            var sender = _userRequestService.GetUser(accessToken);
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
