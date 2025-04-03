using System;
using System.Collections.Generic;

namespace FliesProject.Models;

public partial class QuizWritingSample
{
    public int SampleId { get; set; }

    public int QuestionId { get; set; }

    public string SampleAnswer { get; set; } = null!;

    public virtual QuizQuestion Question { get; set; } = null!;
}
