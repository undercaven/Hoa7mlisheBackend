using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace hoa7mlishe.API.Database.Models;

public partial class CardInfo
{
    public Guid Id { get; set; }

    public string Tag { get; set; } = null!;

    public string Description { get; set; } = null!;

    public int Rarity { get; set; }

    public int Width { get; set; }

    public int Height { get; set; }

    public int SeasonId { get; set; }

    public string? PreviewHash { get; set; }

    public Guid? FileId { get; set; }

    public Guid? LayerOne { get; set; }

    public virtual ICollection<CollectedCard> CollectedCards { get; set; } = new List<CollectedCard>();

    public virtual FileInterface? File { get; set; }

    public virtual CardSeason Season { get; set; } = null!;

    public virtual ICollection<TradeContent> TradeContents { get; set; } = new List<TradeContent>();

    [NotMapped]
    public bool IsShiny { get; internal set; }

    internal CardInfo Clone() => base.MemberwiseClone() as CardInfo;
}
