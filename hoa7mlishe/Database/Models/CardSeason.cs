using System;
using System.Collections.Generic;

namespace hoa7mlishe.API.Database.Models;

public partial class CardSeason
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public bool ActiveSeason { get; set; }

    public virtual ICollection<CardInfo> FileInfos { get; set; } = new List<CardInfo>();
}
