using System;
using System.Collections.Generic;

namespace FliesProject.Models.Entities;

public partial class QuizAnswer
{
    public int AnswerId { get; set; }

    public int QuestionId { get; set; }

    public string AnswerText { get; set; } = null!;

    public bool IsCorrect { get; set; }

    public virtual QuizQuestion Question { get; set; } = null!;
}
