using System;
using System.Collections.Generic;

namespace FliesProject.Models.Entities;

public partial class QuizWritingSubmission
{
    public int SubmissionId { get; set; }

    public int QuestionId { get; set; }

    public int UserId { get; set; }

    public string SubmissionText { get; set; } = null!;

    public string? AisuggestText { get; set; }

    public int? Score { get; set; }

    public DateTime SubmittedAt { get; set; }

    public virtual QuizQuestion Question { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
