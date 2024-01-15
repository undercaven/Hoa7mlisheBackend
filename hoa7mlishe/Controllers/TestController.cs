using hoa7mlishe.API.Services.Interfaces;
using hoa7mlishe.API.Database.Context;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using hoa7mlishe.API.DTO.Cards;
using Microsoft.AspNetCore.Authorization;

namespace hoa7mlishe.API.Controllers
{
    /// <summary>
    /// Контроллер с тестовыми методами
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("CorsPolicy")]
    [Authorize(Roles = "Admin")]
    public class TestController(Hoa7mlisheContext context, IFileService filesvc, IUserRequestService userRequestService) : ControllerBase
    {
        private Hoa7mlisheContext _context = context;
        private IFileService _fileService = filesvc;
        private IUserRequestService _userRequestService = userRequestService;

        /// <summary>
        /// Создает пак карточек (актуальный метод в Dev контроллере
        /// </summary>
        /// <param name="cardPackDto">Модель пака</param>
        /// <returns></returns>
        [HttpPost("chuch")]
        public IActionResult CreatePack([FromForm] CardPackPostDTO cardPackDto)
        {
            //HttpContext.Connection.
            _fileService.CreatePack(cardPackDto);
            return Ok();
        }
    }
}
