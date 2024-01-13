using System;
using System.Collections.Generic;

namespace hoa7mlishe.API.Database.Models;

public partial class CardPack
{
    public Guid Id { get; set; }

    public int Price { get; set; }

    public int CardCount { get; set; }

    public string CardDistribution { get; set; } = null!;

    public string Description { get; set; } = null!;

    public int SeasonId { get; set; }

    public Guid CoverImageId { get; set; }

    public string? Tag { get; set; }

    public string PreviewHash { get; set; } = null!;

    public bool Hidden { get; set; }
}
