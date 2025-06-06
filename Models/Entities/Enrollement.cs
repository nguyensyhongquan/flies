﻿using System;
using System.Collections.Generic;

namespace FliesProject.Models.Entities;

public partial class Enrollement
{
    public int EnrollementId { get; set; }

    public int CourseId { get; set; }

    public int StudentId { get; set; }

    public int MentorId { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? LimitedAt { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();

    public virtual Course Course { get; set; } = null!;

    public virtual ICollection<CourseTransaction> CourseTransactions { get; set; } = new List<CourseTransaction>();

    public virtual User Mentor { get; set; } = null!;

    public virtual ICollection<QuizCompletion> QuizCompletions { get; set; } = new List<QuizCompletion>();

    public virtual User Student { get; set; } = null!;

    public virtual ICollection<UserCourseProgress> UserCourseProgresses { get; set; } = new List<UserCourseProgress>();
}
