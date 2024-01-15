using hoa7mlishe.API.Authorization.DTO;
using hoa7mlishe.API.Authorization.Helpers;
using hoa7mlishe.API.Database;
using hoa7mlishe.API.Database.Context;
using hoa7mlishe.API.Database.Models;
using hoa7mlishe.API.DTO.Users;
using hoa7mlishe.API.Services.Interfaces;
using hoa7mlishe.Helpers;
using hoa7mlishe.Hoa7Enums;
using System.Security.Claims;
using System.Security.Principal;

namespace hoa7mlishe.API.Services
{
    internal class UserRequestService : IUserRequestService
    {
        private Hoa7mlisheContext _context;
        private IFileService _fileService;
        private ILogger<UserRequestService> _logger;

        public UserRequestService(Hoa7mlisheContext context, IFileService fileService, ILogger<UserRequestService> logger)
        {
            _context = context;
            _fileService = fileService;
            _logger = logger;
        }

        public List<UserShortDTO> GetUsersPage(string username, int page, int pageSize)
        {
            List<User> users = string.IsNullOrWhiteSpace(username)
                ? [.. _context.Users]
                : [.. _context.Users.Where(x => x.Username.Contains(username))];

            int arrayStart = (page - 1) * pageSize;
            int offset = arrayStart + pageSize < users.Count ? pageSize : users.Count - arrayStart;

            return users.GetRange(arrayStart, offset)
                .Select(user => user.GetShortDto())
                .ToList();
        }

        public User GetUser(Guid userId)
        {
            return _context.Users.First(x => x.Id == userId);
        }

        public User GetUser(IIdentity identity)
        {
            var claims = identity as ClaimsIdentity;
            var userId = Guid.Parse(claims.FindFirst("NameIdentifier")?.Value);
            return _context.Users.FirstOrDefault(x => x.Id == userId);
        }

        public TokenApiDTO UpdateRefreshToken(TokenApiDTO tokenInfo)
        {
            var tokenPrincipals = AuthorizationHelper.GetPrincipalFromExpiredToken(tokenInfo.AccessToken);
            var userID = Guid.Parse(tokenPrincipals.FindFirst("NameIdentifier")?.Value);

            User user = GetUser(userID);

            if (user?.RefreshToken != tokenInfo.RefreshToken)
            {
                return null;
            }

            var newTokens = AuthorizationHelper.GenerateTokens(user);
            user.RefreshToken = newTokens.RefreshToken;

            _context.Update(user);
            _context.SaveChanges();

            return newTokens;
        }

        public UserResponseDTO LoginUser(UserLoginDTO user)
        {
            byte[] hashedPassword = CryptographyHelper.HashString(user.Password);

            User? userInfo = _context.Users
                .Where(x => x.Login == user.Login && x.Password == hashedPassword)
                .FirstOrDefault();

            if (userInfo is null)
            {
                return null;
            }

            userInfo.RefreshToken = AuthorizationHelper.GenerateRefreshToken();
            _context.SaveChanges();

            UserResponseDTO response = new()
            {
                Id = userInfo.Id,
                Tokens = AuthorizationHelper.GenerateTokens(userInfo),
                Role = userInfo.Role,
                Username = userInfo.Username,
                Mikoins = userInfo.Mikoins,
                AvatarId = userInfo.AvatarId,
            };

            return response;
        }

        public UserResponseDTO RegisterUser(UserRegisterDTO user)
        {
            byte[] hashedPassword = CryptographyHelper.HashString(user.Password);

            if (_context.Users.Any(x => x.Login == user.Login))
            {
                return null;
            }

            var userInfo = new User()
            {
                Id = Guid.NewGuid(),
                Login = user.Login,
                Password = hashedPassword,
                Username = user.Username,
                Role = "user",
                Mikoins = 0
            };

            var tokens = AuthorizationHelper.GenerateTokens(userInfo);
            userInfo.RefreshToken = tokens.RefreshToken;

            _context.Users.Add(userInfo);
            _context.SaveChanges();

            return new UserResponseDTO()
            {
                Id = userInfo.Id,
                Tokens = tokens,
                Role = userInfo.Role,
                Username = userInfo.Username,
            };
        }

        public void UpdateMikoins(
            User user, long mikoins, MikoinUpdateOptions updateOptions = MikoinUpdateOptions.Absolute)
        {
            var transaction = _context.Database.BeginTransaction();

            switch (updateOptions)
            {
                case MikoinUpdateOptions.Additive:
                    user.Mikoins += mikoins;
                    break;
                case MikoinUpdateOptions.Subtractive:
                    user.Mikoins -= mikoins;
                    break;
                case MikoinUpdateOptions.Absolute:
                    user.Mikoins = mikoins;
                    break;
            }

            if (user.Mikoins < 0)
            {
                transaction.Rollback();
                throw new OperationCanceledException();
            }

            _context.Update(user);
            _context.SaveChanges();

            transaction.Commit();
            transaction.Dispose();
        }

        public Guid UploadAvatar(
            User user, IFormFile file)
        {
            string ext = Path.GetExtension(file.FileName);
            Guid avatarId = _fileService.SaveInFileTable(file, ext == ".gif" ? 0 : 300);

            if (user.AvatarId != null)
            {
                _fileService.DeleteFile(user.AvatarId.Value);
            }

            user.AvatarId = avatarId;

            _context.Users.Update(user);
            _context.SaveChanges();

            return avatarId;
        }
    }
}
