using System;
using System.Collections.Generic;

namespace FliesProject.Models;

public partial class Quiz
{
    public int QuizId { get; set; }

    public string Title { get; set; } = null!;

    public string QuizType { get; set; } = null!;

    public string? Content { get; set; }

    public string? MediaUrl { get; set; }

    public int? TimeLimit { get; set; }

    public bool? IsFree { get; set; }

    public decimal? Price { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<QuizComment> QuizComments { get; set; } = new List<QuizComment>();

    public virtual ICollection<QuizQuestion> QuizQuestions { get; set; } = new List<QuizQuestion>();

    public virtual ICollection<QuizTransaction> QuizTransactions { get; set; } = new List<QuizTransaction>();

    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
}
