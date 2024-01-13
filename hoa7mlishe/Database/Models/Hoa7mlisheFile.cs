using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace hoa7mlishe.API.Database.Models;

public partial class Hoa7mlisheFile
{
    public Guid StreamId { get; set; }

    public byte[]? FileStream { get; set; }

    public string Name { get; set; } = null!;

    public HierarchyId PathLocator { get; set; } = null!;

    public HierarchyId? ParentPathLocator { get; set; }

    public string? FileType { get; set; }

    public long? CachedFileSize { get; set; }

    public DateTimeOffset CreationTime { get; set; }

    public DateTimeOffset LastWriteTime { get; set; }

    public DateTimeOffset? LastAccessTime { get; set; }

    public bool IsDirectory { get; set; }

    public bool IsOffline { get; set; }

    public bool IsHidden { get; set; }

    public bool IsReadonly { get; set; }

    public bool? IsArchive { get; set; }

    public bool IsSystem { get; set; }

    public bool IsTemporary { get; set; }

    public virtual ICollection<FileInterface> FileInterfaces { get; set; } = new List<FileInterface>();

    public virtual ICollection<Hoa7mlisheFile> InverseParentPathLocatorNavigation { get; set; } = new List<Hoa7mlisheFile>();

    public virtual Hoa7mlisheFile? ParentPathLocatorNavigation { get; set; }
}
