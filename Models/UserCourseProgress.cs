using System;
using System.Collections.Generic;

namespace FliesProject.Models;

public partial class UserCourseProgress
{
    public int ProgressId { get; set; }

    public int EnrollementId { get; set; }

    public int? CompletedLessons { get; set; }

    public int? CompletedQuizzes { get; set; }

    public int TotalLessons { get; set; }

    public int TotalQuizzes { get; set; }

    public decimal? ProgressPercentage { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Enrollement Enrollement { get; set; } = null!;
}
