using System;
using System.Collections.Generic;

namespace FliesProject.Models.Entities
{
    public partial class LessonQuizMapping
    {
        public int LessonId { get; set; }
        public int QuizId { get; set; }
        public virtual Lesson Lesson { get; set; } = null!;
        public virtual Quiz Quiz { get; set; } = null!;
    }
}
