using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FliesProject.Models.Entities;

public partial class Course
{
    public int CourseId { get; set; }

    [Required(ErrorMessage = "Tiêu đề là bắt buộc")]
    [StringLength(100, ErrorMessage = "Tiêu đề không được vượt quá 100 ký tự")]
    public string Title { get; set; } = null!;

    [Required(ErrorMessage = "Mô tả là bắt buộc")]
    public string Description { get; set; } = null!;

    public string? CoursesPicture { get; set; } // Nullable, as per your model

    public int CreatedBy { get; set; }

    [Required(ErrorMessage = "Thời hạn là bắt buộc")]
    [Range(1, int.MaxValue, ErrorMessage = "Thời hạn phải là số dương")]
    public int Timelimit { get; set; }

    [Required(ErrorMessage = "Giá là bắt buộc")]
    [Range(0, double.MaxValue, ErrorMessage = "Giá phải là số không âm")]
    public decimal Price { get; set; }

    public DateTime CreatedAt { get; set; } // Non-nullable, set in action

    public virtual User? CreatedByNavigation { get; set; }

    public virtual ICollection<Enrollement> Enrollements { get; set; } = new List<Enrollement>();

    public virtual ICollection<Section> Sections { get; set; } = new List<Section>();

    public virtual ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();
}