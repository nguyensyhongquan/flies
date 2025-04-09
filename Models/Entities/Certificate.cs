using System;
using System.Collections.Generic;

namespace FliesProject.Models.Entities;

public partial class Certificate
{
    public int CertificateId { get; set; }

    public int EnrollementId { get; set; }

    public string CertificateCode { get; set; } = null!;

    public DateTime? IssuedAt { get; set; }

    public string? Status { get; set; }

    public virtual Enrollement Enrollement { get; set; } = null!;
}
