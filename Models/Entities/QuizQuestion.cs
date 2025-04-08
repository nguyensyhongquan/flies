using System;
using System.Collections.Generic;

namespace FliesProject.Models.Entities;

public partial class QuizQuestion
{
    public int QuestionId { get; set; }

    public int QuizId { get; set; }

    public string QuestionText { get; set; } = null!;

    public string QuestionType { get; set; } = null!;

    public string? MediaUrl { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Quiz Quiz { get; set; } = null!;

    public virtual ICollection<QuizAnswer> QuizAnswers { get; set; } = new List<QuizAnswer>();

    public virtual ICollection<QuizWritingSample> QuizWritingSamples { get; set; } = new List<QuizWritingSample>();
}
