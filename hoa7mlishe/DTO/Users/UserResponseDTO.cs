namespace hoa7mlishe.API.DTO.Users
{
    public class UserResponseDTO
    {
        public Guid Id { get; set; }
        public string AccessToken { get; set; }
        public string Role { get; set; }
        public string Username { get; set; }
        public string RefreshToken { get; set; }
        public long Mikoins { get; set; }
        public Guid? AvatarId { get; set; }
    }
}
