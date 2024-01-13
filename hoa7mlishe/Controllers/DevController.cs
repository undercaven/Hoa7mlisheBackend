using hoa7mlishe.API.Database.Context;
using hoa7mlishe.API.Database.Models;
using hoa7mlishe.API.DTO.Cards;
using hoa7mlishe.API.DTO.Files;
using hoa7mlishe.API.Services.Interfaces;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace hoa7mlishe.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("CorsPolicy")]
    public class DevController : ControllerBase
    {
        private Hoa7mlisheContext _context;
        private IFileService _fileService;
        private IUserRequestService _userRequestService;

        public DevController(Hoa7mlisheContext context, IFileService filesvc, IUserRequestService userRequestService)
        {
            _context = context;
            _fileService = filesvc;
            _userRequestService = userRequestService;
        }

        [HttpPut("packs/{packId}/changeVisibility")]
        public IActionResult ChangePackVisibility(
            Guid packId,
            [FromHeader] string accessToken)
        {
            var user = _userRequestService.GetUser(accessToken);

            if (user?.Role != "admin")
            {
                return Unauthorized();
            }

            var pack = _context.CardPacks.Single(x => x.Id == packId);
            pack.Hidden = !pack.Hidden;

            _context.CardPacks.Update(pack);
            _context.SaveChanges();

            return Ok();
        }

        [HttpPost("changeCardImage/{id}")]
        public IActionResult ChangeCardImage(
            [FromForm] FileDTO file,
            Guid id,
            [FromHeader] string accessToken)
        {
            var user = _userRequestService.GetUser(accessToken);

            if (user?.Role != "admin")
            {
                return Unauthorized();
            }

            var card = _context.FileInfos.Single(x => x.Id == id);
            var fi = _context.FileInterfaces.Single(x => x.RecordId == card.FileId);
            var fr = _context.Hoa7mlisheFiles.Single(x => x.PathLocator == fi.PathLocator);

            _fileService.SaveInFileTable(file.File, filename: fr.Name);

            return Ok();
        }

        [HttpPost("linkFiles")]
        public IActionResult LinkFileInterface(
            [FromForm] string filename,
            [FromHeader] string accessToken)
        {
            var user = _userRequestService.GetUser(accessToken);

            if (user?.Role != "admin")
            {
                return Unauthorized();
            }

            Hoa7mlisheFile filerecord = _context.Hoa7mlisheFiles.Single(x => x.Name == filename);
            FileInterface fi = new()
            {
                RecordId = Guid.NewGuid(),
                PathLocator = filerecord.PathLocator,
            };

            _context.FileInterfaces.Add(fi);
            _context.SaveChanges();

            return Ok(fi.RecordId);
        }

        /// <summary>
        /// Добавляет файл в базу
        /// </summary>
        /// <param name="file">Файл</param>
        /// <returns>ID созданного файла</returns>
        /// <response code="200">Ok</response>
        /// <response code="400">Неверный формат файла</response>
        [HttpPost("lore")]
        public IActionResult PostFile(
            [FromForm] CardDTO file,
            [FromHeader] string accessToken)
        {
            try
            {
                var user = _userRequestService.GetUser(accessToken);

                if (user?.Role != "admin")
                {
                    return Unauthorized();
                }

                Guid fileId = _fileService.PostCard(file);
                return Ok(fileId);
            }
            catch (IOException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("packs/{packId}")]
        public IActionResult DeletePack(
            Guid packId,
            [FromHeader] string accessToken)
        {
            var user = _userRequestService.GetUser(accessToken);
            if (user?.Role != "admin")
            {
                return Unauthorized();
            }

            var packToRemove = _context.CardPacks.SingleOrDefault(x => x.Id == packId);

            if (packToRemove is not null)
            {
                _context.CardPacks.Remove(packToRemove);
                _context.SaveChanges();
            }

            return Ok();
        }
        
        [HttpDelete("cards")]
        public IActionResult DeleteCards(
            List<Guid> cardIds,
            [FromHeader] string accessToken)
        {
            var user = _userRequestService.GetUser(accessToken);
            if (user?.Role != "admin")
            {
                return Unauthorized();
            }

            foreach (var cardId in cardIds)
            {
                var cardToRemove = _context.FileInfos.SingleOrDefault(x => x.Id == cardId);

                if (cardToRemove is not null)
                {
                    _context.FileInfos.Remove(cardToRemove);
                    _context.SaveChanges();
                }
            }

            return Ok();
        }

        [HttpPost("packs")]
        public IActionResult CreatePack(
            [FromForm] CardPackPostDTO packDto,
            [FromHeader] string accessToken)
        {
            try
            {
                var user = _userRequestService.GetUser(accessToken);

                if (user?.Role != "admin")
                {
                    return Unauthorized();
                }

                _fileService.CreatePack(packDto);

                return Ok();
            }
            catch (IOException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Добавляет цитату ивонова в базу
        /// </summary>
        /// <param name="quote"> текст цитаты</param>
        /// <returns>Результат создания</returns>
        [HttpPost("quotes")]
        public IActionResult PostIvonovQuote(
            [FromBody] string quote,
            [FromHeader] string accessToken)
        {
            var user = _userRequestService.GetUser(accessToken);
            if (user?.Role != "admin")
            {
                return Unauthorized();
            }
            var ivonovQuote = new IvonovQuote()
            {
                Quote = quote,
                Id = Guid.NewGuid()
            };

            _context.IvonovQuotes.Add(ivonovQuote);
            _context.SaveChanges();

            return Ok();
        }

        /// <summary>
        /// Создает новый счетчик
        /// </summary>
        /// <param name="dClock">счетчик</param>
        /// <returns>Результат создания</returns>
        /// <response code="201">Счетчик успешно создан</response>
        /// <response code="409">Счетчик с таким именем уже существует</response>
        [HttpPost("DeathClock")]
        public IActionResult PostDeathClock(
            [FromBody] DeathClock dClock,
            [FromHeader] string accessToken)
        {
            var user = _userRequestService.GetUser(accessToken);
            if (user?.Role != "admin")
            {
                return Unauthorized();
            }

            var deathClock = _context.DeathClocks.Where(x => x.Name == dClock.Name).FirstOrDefault();

            if (deathClock is not null)
            {
                return Conflict("Часы с таким именем уже существуют!");
            }

            _context.DeathClocks.Add(dClock);
            _context.SaveChanges();

            return CreatedAtAction(nameof(PostDeathClock), dClock);
        }
    }
}
