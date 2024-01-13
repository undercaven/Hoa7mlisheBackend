using System;
using System.Collections.Generic;

namespace hoa7mlishe.API.Database.Models;

public partial class TradeOffer
{
    public Guid Id { get; set; }

    public Guid SenderId { get; set; }

    public Guid ReceiverId { get; set; }

    public DateTime SendTime { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? ConfirmationTime { get; set; }

    public int Mikoins { get; set; }

    public virtual User Receiver { get; set; } = null!;

    public virtual User Sender { get; set; } = null!;

    public virtual ICollection<TradeContent> TradeContents { get; set; } = new List<TradeContent>();
}
