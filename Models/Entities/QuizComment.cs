using System;
using System.Collections.Generic;

namespace FliesProject.Models.Entities;

public partial class QuizComment
{
    public int CommentId { get; set; }

    public int LessonId { get; set; }

    public int UserId { get; set; }

    public string CommentText { get; set; } = null!;

    public int? ParentCommentId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<QuizComment> InverseParentComment { get; set; } = new List<QuizComment>();

    public virtual Lesson Lesson { get; set; } = null!;

    public virtual QuizComment? ParentComment { get; set; }

    public virtual User User { get; set; } = null!;
}
