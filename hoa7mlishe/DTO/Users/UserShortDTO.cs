namespace hoa7mlishe.API.DTO.Users
{
    public class UserShortDTO
    {
        public string Username { get; set; }
        public string Role { get; set; }
        public long Mikoins { get; set; }
        public Guid Id { get; set; }
        public Guid? AvatarId { get; set; }
    }
}
