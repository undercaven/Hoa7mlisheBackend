using hoa7mlishe.API.Database.Context;
using hoa7mlishe.API.Database.Models;
using hoa7mlishe.API.DTO.Users;
using hoa7mlishe.API.Services.Interfaces;
using hoa7mlishe.Helpers;
using hoa7mlishe.Hoa7Enums;

namespace hoa7mlishe.API.Services
{
    internal class UserRequestService : IUserRequestService
    {
        private Hoa7mlisheContext _context;
        private IFileService _fileService;

        public UserRequestService(Hoa7mlisheContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }
        public List<UserShortDTO> GetUsersPage(string username, int page, int pageSize)
        {
            List<User> users;

            if (string.IsNullOrWhiteSpace(username))
            {
                users = _context.Users.ToList();
            }
            else
            {
                users = _context.Users.Where(x => x.Username.Contains(username)).ToList();
            }

            int arrayStart = (page - 1) * pageSize;
            int offset;

            if (arrayStart + pageSize < users.Count)
            {
                offset = pageSize;
            }
            else
            {
                offset = users.Count - arrayStart;
            }

            var result = new List<UserShortDTO>();
            foreach (var user in users.GetRange(arrayStart, offset))
            {
                result.Add(user.GetShortDto());
            }

            return result;
        }

        public User GetUser(Guid userId)
        {
            return _context.Users.Where(x => x.Id == userId).FirstOrDefault();
        }

        public User GetUser(string accessToken, int hoursOffset = 3)
        {
            Guid userId = AuthorizationHelper.DecypherToken(accessToken, hoursOffset);

            return _context.Users.Where(x => x.Id == userId).FirstOrDefault();
        }

        public string UpdateRefreshToken(ref string refreshToken)
        {
            User user = GetUser(refreshToken, 140);

            if (user is null)
            {
                return null;
            }

            refreshToken = AuthorizationHelper.GenerateToken(Guid.NewGuid(), 140);
            user.RefreshToken = refreshToken;

            _context.Update(user);
            _context.SaveChanges();

            return AuthorizationHelper.GenerateToken(user.Id, 3);
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

            userInfo.RefreshToken = AuthorizationHelper.GenerateToken(userInfo.Id, 140);
            _context.SaveChanges();

            UserResponseDTO response = new()
            {
                Id = userInfo.Id,
                AccessToken = AuthorizationHelper.GenerateToken(userInfo.Id, 3),
                Role = userInfo.Role,
                Username = userInfo.Username,
                RefreshToken = userInfo.RefreshToken,
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

            userInfo.RefreshToken = AuthorizationHelper.GenerateToken(userInfo.Id, 140);

            _context.Users.Add(userInfo);
            _context.SaveChanges();

            return new UserResponseDTO()
            {
                Id = userInfo.Id,
                AccessToken = AuthorizationHelper.GenerateToken(userInfo.Id, 3),
                Role = userInfo.Role,
                Username = userInfo.Username,
                RefreshToken = userInfo.RefreshToken,
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
