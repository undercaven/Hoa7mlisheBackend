using System;
using System.Collections.Generic;

namespace hoa7mlishe.API.Database.Models;

public partial class TradeContent
{
    public Guid TradeId { get; set; }

    public Guid CardId { get; set; }

    public bool Request { get; set; }

    public int Count { get; set; }

    public bool IsShiny { get; set; }

    public virtual CardInfo Card { get; set; } = null!;

    public virtual TradeOffer Trade { get; set; } = null!;
}
