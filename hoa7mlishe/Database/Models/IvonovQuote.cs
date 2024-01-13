using System;
using System.Collections.Generic;

namespace hoa7mlishe.API.Database.Models;

public partial class IvonovQuote
{
    public Guid Id { get; set; }

    public string Quote { get; set; } = null!;
}
