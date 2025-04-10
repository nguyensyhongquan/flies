using System;
using System.Collections.Generic;

namespace FliesProject.Models.Entities;

public partial class CourseTransaction
{
    public int TransactionId { get; set; }

    public int EnrollementId { get; set; }

    public decimal Amount { get; set; }

    public DateTime? TransactionDate { get; set; }

    public string TransactionType { get; set; } = null!;

    public virtual Enrollement Enrollement { get; set; } = null!;
}
