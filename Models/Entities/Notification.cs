using System;
using System.Collections.Generic;

namespace FliesProject.Models.Entities;

public partial class Notification
{
    public int NotificationId { get; set; }

    public int UserId { get; set; }

    public string Title { get; set; } = null!;

    public string Message { get; set; } = null!;

    public string NotificationType { get; set; } = null!;

    public bool? IsRead { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? ReadAt { get; set; }

    public string? Status { get; set; }

    public bool? IsUrgent { get; set; }

    public string? Link { get; set; }

    public int? SenderId { get; set; }

    public virtual User? Sender { get; set; }

    public virtual User User { get; set; } = null!;
}
