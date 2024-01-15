using hoa7mlishe.API.Authorization.DTO;
using hoa7mlishe.API.Authorization.Helpers;
using hoa7mlishe.API.Database.Context;
using hoa7mlishe.API.Database.Models;
using hoa7mlishe.API.DTO.Files;
using hoa7mlishe.API.DTO.Users;
using hoa7mlishe.API.Mail;
using hoa7mlishe.API.Services;
using hoa7mlishe.API.Services.Interfaces;
using hoa7mlishe.Helpers;
using hoa7mlishe.Hoa7Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace hoa7mlishe.API.Controllers
{
    /// <summary>
    /// Контроллер для операций работы с пользователями
    /// </summary>
    /// <param name="context">Контекст БД</param>
    /// <param name="userRequestService">Сервис для работы с пользователями</param>
    /// <param name="mailService">Почтовый сервис</param>
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("CorsPolicy")]
    [Authorize]
    public class UsersController(Hoa7mlisheContext context, IUserRequestService userRequestService, IMailService mailService) : ControllerBase
    {
        private Hoa7mlisheContext _context = context;
        private IUserRequestService _userRequestService = userRequestService;
        private IMailService _mailService = mailService;

        /// <summary>
        /// Обновляет устаревший токен
        /// </summary>
        /// <param name="tokenInfo">Токены пользователя</param>
        /// <returns>Новая пара токенов</returns>
        [AllowAnonymous]
        [HttpGet("refreshToken")]
        public IActionResult RefreshToken(TokenApiDTO tokenInfo)
        {
            var newTokens = _userRequestService.UpdateRefreshToken(tokenInfo);

            if (newTokens is null)
            {
                return Unauthorized();
            }

            return Ok(newTokens);
        }

        /// <summary>
        /// Авторизация пользователя
        /// </summary>
        /// <param name="user"> данные пользователя </param>
        /// <returns>accessToken</returns>
        [HttpPost("login")]
        [AllowAnonymous]
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
        [AllowAnonymous]
        public IActionResult RegisterUser(
            [FromForm] UserRegisterDTO user)
        {
            UserResponseDTO response = _userRequestService.RegisterUser(user);

            return response is null ? Conflict() : Ok(response);
        }

        /// <summary>
        /// Меняет фото профиля пользователя
        /// </summary>
        /// <param name="avatar">Новое фото</param>
        /// <returns></returns>
        [HttpPost("uploadAvatar")]
        public IActionResult UploadUserAvatar(
            [FromForm] FileDTO avatar)
        {
            var user = _userRequestService.GetUser(User.Identity);

            if (user is null)
            {
                return Unauthorized();
            }

            return Ok(_userRequestService.UploadAvatar(user, avatar.File));
        }

        /// <summary>
        /// Возвращает краткую информацию о требуемом пользователе
        /// </summary>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <returns></returns>
        [HttpGet("{userId}")]
        [AllowAnonymous]
        public IActionResult GetUserInfo(
            Guid userId)
        {
            User requestedUser = _userRequestService.GetUser(userId);

            return requestedUser is null ? NotFound() : Ok(new { requestedUser.Username, requestedUser.Role, requestedUser.Mikoins, requestedUser.AvatarId });
        }

        /// <summary>
        /// Получает количество зарегистрированных пользователей
        /// </summary>
        /// <param name="username">Если параметр указан, будет произведен поиск пользователей, чьи имена содержат данную подстроку</param>
        /// <returns></returns>
        [HttpGet("search")]
        [AllowAnonymous]
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

        /// <summary>
        /// Получает страницу с пользователями
        /// </summary>
        /// <param name="username">Если параметр указан, будет произведен поиск пользователей, чьи имена содержат данную подстроку</param>
        /// <param name="page">Номер страницы</param>
        /// <param name="pageSize">Размер страницы</param>
        /// <returns></returns>
        [HttpGet("search/{page}")]
        [AllowAnonymous]
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
        /// <returns>Инфо о пользователе</returns>
        [HttpGet]
        public IActionResult GetMe()
        {
            User user = _userRequestService.GetUser(User.Identity);

            if (user is null)
            {
                return Unauthorized();
            }

            var ip = string.Format("Sender IP: {0}, {1}\n", user.Username, Request.HttpContext.Connection.RemoteIpAddress);
            Console.WriteLine(ip);
            return Ok(new { user.Username, user.Role, user.Mikoins, user.Id, user.AvatarId });
        }

        /// <summary>
        /// Обновляет микоины пользователя
        /// </summary>
        /// <param name="mikoins">новое количество микоинов</param>
        /// <returns></returns>
        [HttpPost("mikoins")]
        public IActionResult UpdateUserMikoins(
            long mikoins)
        {
            User user = _userRequestService.GetUser(User.Identity);

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
        /// <returns></returns>
        [HttpGet("mikoins")]
        [AllowAnonymous]
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
        /// <returns></returns>
        [HttpPost("update/{id}")]
        public IActionResult UpdateData(
            Guid id,
            [FromQuery] UserRegisterDTO userRegister)
        {
            try
            {
                User user = _userRequestService.GetUser(User.Identity);

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
        
        #endregion
    }
}
