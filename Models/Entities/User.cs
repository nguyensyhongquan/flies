using System;
using System.Collections.Generic;

namespace FliesProject.Models.Entities;

public partial class User
{
    public int UserId { get; set; }

    public string Email { get; set; } = null!;

    public string Passwordhash { get; set; } = null!;

    public string? Fullname { get; set; }

    public string? AvatarUrl { get; set; }

    public string Role { get; set; } = null!;

    public decimal? Balance { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? LastLogin { get; set; }

    public string? Status { get; set; }

    public string? Gender { get; set; }

    public string Username { get; set; } = null!;

    public DateTime? Birthday { get; set; }

    public string? Address { get; set; }

    public string? PhoneNumber { get; set; }

    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();

    public virtual ICollection<Enrollement> EnrollementMentors { get; set; } = new List<Enrollement>();

    public virtual ICollection<Enrollement> EnrollementStudents { get; set; } = new List<Enrollement>();

    public virtual ICollection<Notification> NotificationSenders { get; set; } = new List<Notification>();

    public virtual ICollection<Notification> NotificationUsers { get; set; } = new List<Notification>();

    public virtual ICollection<PaymentOrder> PaymentOrders { get; set; } = new List<PaymentOrder>();

    public virtual ICollection<QuizComment> QuizComments { get; set; } = new List<QuizComment>();

    public virtual ICollection<QuizTransaction> QuizTransactions { get; set; } = new List<QuizTransaction>();

    public virtual ICollection<QuizWritingSubmission> QuizWritingSubmissions { get; set; } = new List<QuizWritingSubmission>();
}
