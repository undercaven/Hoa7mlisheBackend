using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace hoa7mlishe.API.Database.Models;

public partial class FileInterface
{
    public Guid RecordId { get; set; }

    public HierarchyId PathLocator { get; set; } = null!;

    public virtual ICollection<CardInfo> FileInfos { get; set; } = new List<CardInfo>();

    public virtual Hoa7mlisheFile PathLocatorNavigation { get; set; } = null!;
}
