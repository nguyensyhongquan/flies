using System;
using System.Collections.Generic;

namespace FliesProject.Models.Entities;

public partial class LessonCompletion
{
    public int CompletionId { get; set; }

    public int EnrollementId { get; set; }

    public int LessonId { get; set; }

    public DateTime CompletedAt { get; set; }

    public virtual Enrollement Enrollement { get; set; } = null!;

    public virtual Lesson Lesson { get; set; } = null!;
}
