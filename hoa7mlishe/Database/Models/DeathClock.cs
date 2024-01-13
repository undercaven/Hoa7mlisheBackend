using System;
using System.Collections.Generic;

namespace hoa7mlishe.API.Database.Models;

public partial class DeathClock
{
    public string Name { get; set; } = null!;

    public DateTime DeathTime { get; set; }
}
