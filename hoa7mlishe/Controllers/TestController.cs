using hoa7mlishe.API.Services.Interfaces;
using hoa7mlishe.API.Database.Context;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using hoa7mlishe.API.DTO.Cards;

namespace hoa7mlishe.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("CorsPolicy")]
    public class TestController : ControllerBase
    {
        private Hoa7mlisheContext _context;
        private IFileService _fileService;
        private IUserRequestService _userRequestService;

        public TestController(Hoa7mlisheContext context, IFileService filesvc, IUserRequestService userRequestService)
        {
            _context = context;
            _fileService = filesvc;
            _userRequestService = userRequestService;
        }

        [HttpPost("chuch")]
        public IActionResult CreatePack([FromForm] CardPackPostDTO cardPackDto)
        {
            //HttpContext.Connection.
            _fileService.CreatePack(cardPackDto);
            return Ok();
        }
    }
}
