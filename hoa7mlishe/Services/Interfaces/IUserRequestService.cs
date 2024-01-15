using hoa7mlishe.API.Authorization.DTO;
using hoa7mlishe.API.Database.Models;
using hoa7mlishe.API.DTO.Users;
using hoa7mlishe.Hoa7Enums;
using System.Security.Claims;
using System.Security.Principal;

namespace hoa7mlishe.API.Services.Interfaces
{
    public interface IUserRequestService
    {
        public List<UserShortDTO> GetUsersPage(string username, int page, int pageSize);

        public User GetUser(Guid userId);

        public User GetUser(IIdentity claims);

        public TokenApiDTO UpdateRefreshToken(TokenApiDTO tokenInfo);

        public UserResponseDTO LoginUser(UserLoginDTO user);

        public UserResponseDTO RegisterUser(UserRegisterDTO user);

        public void UpdateMikoins(User user, long mikoins, MikoinUpdateOptions updateOptions = MikoinUpdateOptions.Absolute);

        public Guid UploadAvatar(User user, IFormFile file);
    }
}
