using System;
using System.Collections.Generic;

namespace FliesProject.Models;

public partial class QuizTransaction
{
    public int QuiztransactionId { get; set; }

    public int UserId { get; set; }

    public int QuizId { get; set; }

    public decimal Amount { get; set; }

    public DateTime? TransactionDate { get; set; }

    public virtual Quiz Quiz { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
