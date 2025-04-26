using System;
using System.Collections.Generic;

namespace FliesProject.Models.Entities;

public partial class QuizCompletion
{
    public int CompletionId { get; set; }

    public int EnrollementId { get; set; }

    public int QuizId { get; set; }

    public int Score { get; set; }

    public DateTime CompletedAt { get; set; }

    public virtual Enrollement Enrollement { get; set; } = null!;

    public virtual Quiz Quiz { get; set; } = null!;
}
