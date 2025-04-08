using System;
using System.Collections.Generic;

namespace FliesProject.Models.Entities;

public partial class Section
{
    public int SectionId { get; set; }

    public int CourseId { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public int? Positition { get; set; }

    public DateTime? CreateAt { get; set; }

    public virtual Course Course { get; set; } = null!;

    public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
}
