using hoa7mlishe.API.Database.Context;
using hoa7mlishe.API.Database.Models;
using hoa7mlishe.API.DTO.Cards;
using hoa7mlishe.API.DTO.Files;
using hoa7mlishe.API.Mail;
using hoa7mlishe.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace hoa7mlishe.API.Controllers
{
    /// <summary>
    /// Контроллер с методами администратора
    /// </summary>
    /// <param name="context">Контекст БД</param>
    /// <param name="filesvc">Файловый сервис</param>
    /// <param name="userRequestService">Сервис для работы с пользователями</param>
    /// <param name="mailService">Почтовый сервис</param>
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("CorsPolicy")]
    [Authorize(Roles = "admin")]
    public class DevController(Hoa7mlisheContext context, IFileService filesvc, IUserRequestService userRequestService, IMailService mailService) : ControllerBase
    {
        private Hoa7mlisheContext _context = context;
        private IFileService _fileService = filesvc;
        private IUserRequestService _userRequestService = userRequestService;
        private IMailService _mailService = mailService;

        /// <summary>
        /// Изменяет видимость пака карточек
        /// </summary>
        /// <param name="packId">Идентификатор пака</param>
        /// <returns></returns>
        [HttpPut("packs/{packId}/changeVisibility")]
        public IActionResult ChangePackVisibility(
            Guid packId)
        {
            var pack = _context.CardPacks.Single(x => x.Id == packId);
            pack.Hidden = !pack.Hidden;

            _context.CardPacks.Update(pack);
            _context.SaveChanges();

            return Ok();
        }

        /// <summary>
        /// Изменяет фото карточки
        /// </summary>
        /// <param name="file">Новое фото</param>
        /// <param name="id">Идентификатор карточки</param>
        /// <returns></returns>
        [HttpPost("changeCardImage/{id}")]
        public IActionResult ChangeCardImage(
            [FromForm] FileDTO file,
            Guid id)
        {
            var card = _context.FileInfos.Single(x => x.Id == id);
            var fi = _context.FileInterfaces.Single(x => x.RecordId == card.FileId);
            var fr = _context.Hoa7mlisheFiles.Single(x => x.PathLocator == fi.PathLocator);

            _fileService.SaveInFileTable(file.File, filename: fr.Name);

            return Ok();
        }

        /// <summary>
        /// Связывает файловый интерфейс с записью о карточке
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        [HttpPost("linkFiles")]
        public IActionResult LinkFileInterface(
            [FromForm] string filename)
        {
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
            [FromForm] CardDTO file)
        {
            try
            {
                Guid fileId = _fileService.PostCard(file);
                return Ok(fileId);
            }
            catch (IOException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Удаляет пак карточек из базы
        /// </summary>
        /// <param name="packId">Идентификатор пака</param>
        /// <returns></returns>
        [HttpDelete("packs/{packId}")]
        public IActionResult DeletePack(
            Guid packId)
        {
            var packToRemove = _context.CardPacks.SingleOrDefault(x => x.Id == packId);

            if (packToRemove is not null)
            {
                _context.CardPacks.Remove(packToRemove);
                _context.SaveChanges();
            }

            return Ok();
        }
        
        /// <summary>
        /// Удаляет карточки из базы
        /// </summary>
        /// <param name="cardIds">Идентификаторы карточек</param>
        /// <returns></returns>
        [HttpDelete("cards")]
        public IActionResult DeleteCards(
            List<Guid> cardIds)
        {
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

        /// <summary>
        /// Создает пак карточек
        /// </summary>
        /// <param name="packDto">Модель пака</param>
        /// <returns></returns>
        [HttpPost("packs")]
        public IActionResult CreatePack(
            [FromForm] CardPackPostDTO packDto)
        {
            try
            {
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
        /// <param name="quote">Текст цитаты</param>
        /// <returns></returns>
        [HttpPost("quotes")]
        public IActionResult PostIvonovQuote(
            [FromBody] string quote)
        {
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
        /// <param name="dClock">Счетчик</param>
        /// <returns>Результат создания</returns>
        /// <response code="201">Счетчик успешно создан</response>
        /// <response code="409">Счетчик с таким именем уже существует</response>
        [HttpPost("DeathClock")]
        public IActionResult PostDeathClock(
            [FromBody] DeathClock dClock)
        {
            var deathClock = _context.DeathClocks.Where(x => x.Name == dClock.Name).FirstOrDefault();

            if (deathClock is not null)
            {
                return Conflict("Часы с таким именем уже существуют!");
            }

            _context.DeathClocks.Add(dClock);
            _context.SaveChanges();

            return CreatedAtAction(nameof(PostDeathClock), dClock);
        }

        /// <summary>
        /// ТЕСТОВЫЙ МЕТОД
        /// Отправляет почтовое сообщение
        /// </summary>
        /// <param name="caption">Заголовок сообщения</param>
        /// <param name="message"></param>
        /// <param name="sendTo"></param>
        /// <param name="attachments"></param>
        /// <returns></returns>
        [HttpPost("sendMailMessage")]
        public async Task<IActionResult> SendTestMessage(
            [FromForm] string caption,
            [FromForm] string message,
            [FromForm] string[] sendTo,
            [FromForm] IFormFile[] attachments)
        {
            var pars = new MailParameters(caption, message);
            pars.AddAttachments(attachments);
            pars.AddRecipients(sendTo);
            await _mailService.SendMessage(pars);

            return Ok();
        }
    }
}
