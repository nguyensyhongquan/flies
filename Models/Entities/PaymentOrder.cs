using System;
using System.Collections.Generic;

namespace FliesProject.Models.Entities;

public partial class PaymentOrder
{
    public string OrderId { get; set; } = null!;

    public int UserId { get; set; }

    public decimal Amount { get; set; }

    public string Status { get; set; } = null!;

    public string? TransactionRef { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
