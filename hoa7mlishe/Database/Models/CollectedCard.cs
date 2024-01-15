using System;
using System.Collections.Generic;

namespace hoa7mlishe.API.Database.Models;

public partial class CollectedCard
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid CardId { get; set; }

    public int Count { get; set; }

    public bool IsShiny { get; set; }

    public virtual CardInfo Card { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
