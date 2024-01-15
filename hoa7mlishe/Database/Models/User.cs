using hoa7mlishe.API.DTO.Users;
using System;
using System.Collections.Generic;

namespace hoa7mlishe.API.Database.Models;

public partial class User
{
    public Guid Id { get; set; }

    public string Login { get; set; } = null!;

    public byte[] Password { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string Role { get; set; } = null!;

    public string? RefreshToken { get; set; }

    public long Mikoins { get; set; }

    public Guid? AvatarId { get; set; }

    public virtual ICollection<TradeOffer> TradeOfferReceivers { get; set; } = new List<TradeOffer>();

    public virtual ICollection<TradeOffer> TradeOfferSenders { get; set; } = new List<TradeOffer>();

    

}
