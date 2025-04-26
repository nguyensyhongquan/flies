using System;
using System.Collections.Generic;

namespace FliesProject.Models.Entities;

public partial class Lesson
{
    public int LessonId { get; set; }

    public int SectionId { get; set; }

    public string Title { get; set; } = null!;

    public string VideoUrl { get; set; } = null!;

    public int? Duration { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Section Section { get; set; } = null!;

    public virtual ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();
}
