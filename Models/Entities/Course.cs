using System;
using System.Collections.Generic;

namespace FliesProject.Models.Entities;

public partial class Course
{
    public int CourseId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string? CoursesPicture { get; set; }

    public int CreatedBy { get; set; }

    public int? Timelimit { get; set; }

    public decimal? Price { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User CreatedByNavigation { get; set; } = null!;

    public virtual ICollection<Enrollement> Enrollements { get; set; } = new List<Enrollement>();

    public virtual ICollection<Section> Sections { get; set; } = new List<Section>();

    public virtual ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();
}