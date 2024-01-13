using hoa7mlishe.API.Database.Context;
using hoa7mlishe.API.Database.Models;
using hoa7mlishe.API.DTO.Files;
using hoa7mlishe.API.DTO.Users;
using hoa7mlishe.API.Mail;
using hoa7mlishe.API.Services;
using hoa7mlishe.API.Services.Interfaces;
using hoa7mlishe.Helpers;
using hoa7mlishe.Hoa7Enums;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace hoa7mlishe.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("CorsPolicy")]
    public class UsersController : ControllerBase
    {
        private Hoa7mlisheContext _context;
        private IUserRequestService _userRequestService;
        private IMailService _mailService;

        public UsersController(Hoa7mlisheContext context, IUserRequestService userRequestService, IMailService mailService)
        {
            _context = context;
            _userRequestService = userRequestService;
            _mailService = mailService;
        }

        /// <summary>
        /// Обновляет accessToken пользователя
        /// </summary>
        /// <param name="refreshToken"></param>
        /// <returns></returns>
        [HttpGet("refreshToken")]
        public IActionResult RefreshToken(string refreshToken)
        {
            string accessToken = _userRequestService.UpdateRefreshToken(ref refreshToken);

            if (accessToken is null)
            {
                return Unauthorized();
            }

            return Ok(new
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            });
        }

        /// <summary>
        /// Авторизация пользователя
        /// </summary>
        /// <param name="user"> данные пользователя </param>
        /// <returns>accessToken</returns>
        [HttpPost("login")]
        public IActionResult LoginUser(
            [FromBody] UserLoginDTO user)
        {
            var response = _userRequestService.LoginUser(user);

            return response is null ? Unauthorized() : Ok(response);
        }

        /// <summary>
        /// Регистрация пользователя
        /// </summary>
        /// <param name="user"> данные пользователя</param>
        /// <returns>accessToken</returns>
        [HttpPost("register")]
        public IActionResult RegisterUser(
            [FromForm] UserRegisterDTO user)
        {
            UserResponseDTO response = _userRequestService.RegisterUser(user);

            return response is null ? Conflict() : Ok(response);
        }

        [HttpPost("uploadAvatar")]
        public IActionResult UploadUserAvatar(
            [FromForm] FileDTO avatar,
            [FromHeader] string accessToken)
        {
            var user = _userRequestService.GetUser(accessToken);

            if (user is null)
            {
                return Unauthorized();
            }

            return Ok(_userRequestService.UploadAvatar(user, avatar.File));
        }

        /// <summary>
        /// Возвращает имя и роль пользователя
        /// </summary>
        /// <param name="accessToken"></param>
        /// <returns>Инфо о пользователе</returns>
        [HttpGet("{userId}")]
        public IActionResult GetUserInfo(
            Guid userId)
        {
            User requestedUser = _userRequestService.GetUser(userId);

            return requestedUser is null ? NotFound() : Ok(new { requestedUser.Username, requestedUser.Role, requestedUser.Mikoins, requestedUser.AvatarId });
        }

        [HttpGet("search")]
        public IActionResult GetUserList(
            string? username
            )
        {
            if (string.IsNullOrEmpty(username))
            {
                return Ok(_context.Users.Count());
            }

            return Ok(_context.Users.Where(x => x.Username.Contains(username)).Count());
        }

        [HttpGet("decypherToken")]
        public IActionResult DecypherToken(
            string token)
        {
            DateTime when = DateTime.Now;
            Guid g = AuthorizationHelper.DecypherToken(token, ref when);
            string whenstr = when.ToLongDateString();
            return Ok(new { g, whenstr });
        }

        [HttpGet("search/{page}")]
        public IActionResult GetUsersPage(
            string? username,
            int page,
            int pageSize)
        {
            return Ok(_userRequestService.GetUsersPage(username, page, pageSize));
        }

        /// <summary>
        /// Возвращает имя и роль текущего пользователя пользователя
        /// </summary>
        /// <param name="accessToken"></param>
        /// <returns>Инфо о пользователе</returns>
        [HttpGet]
        public IActionResult GetMe(
            [FromHeader] string accessToken)
        {
            User user = _userRequestService.GetUser(accessToken);

            if (user is not null)
            {
                var ip = String.Format("Sender IP: {0}, {1}\n", user.Username, Request.HttpContext.Connection.RemoteIpAddress);
                Console.WriteLine(ip);
            }

            return user is null ? Unauthorized() : Ok(new { user.Username, user.Role, user.Mikoins, user.Id, user.AvatarId });
        }

        /// <summary>
        /// Обновляет микоины пользователя
        /// </summary>
        /// <param name="mikoins">новое количество микоинов</param>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        [HttpPost("mikoins")]
        public IActionResult UpdateUserMikoins(
            long mikoins,
            [FromHeader] string accessToken)
        {
            User user = _userRequestService.GetUser(accessToken);

            if (user is null)
            {
                return Unauthorized();
            }

            try
            {
                _userRequestService.UpdateMikoins(user, mikoins, MikoinUpdateOptions.Additive);
            }
            catch
            {
                return Conflict();
            }

            return Ok(user.Mikoins);
        }

        /// <summary>
        /// Получает микоины пользователя
        /// </summary>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        [HttpGet("mikoins")]
        public IActionResult GetUserMikoins(
            Guid userId)
        {
            User receiver = _userRequestService.GetUser(userId);

            return Ok(receiver.Mikoins);
        }

        /// <summary>
        /// Обновляет данные пользователя
        /// </summary>
        /// <param name="id">id пользователя</param>
        /// <param name="userRegister">новые данные</param>
        /// <param name="accessToken">токен</param>
        /// <returns></returns>
        [HttpPost("update/{id}")]
        public IActionResult UpdateData(
            Guid id,
            [FromQuery] UserRegisterDTO userRegister,
            [FromHeader] string accessToken)
        {
            try
            {
                User user = _userRequestService.GetUser(accessToken);

                if (user?.Role != "admin")
                {
                    return Unauthorized();
                }

                User requestedUser = _userRequestService.GetUser(id);
                requestedUser.Username = userRegister.Username;
                requestedUser.Password = CryptographyHelper.HashString(userRegister.Password);

                _context.Users.Update(requestedUser);
                _context.SaveChanges();
                return Ok();
            }
            catch (Exception ex)
            {
                return Conflict(ex.Message);
            }
        }

        #region test
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
        #endregion
    }
}
