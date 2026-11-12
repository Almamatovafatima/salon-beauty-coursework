using System;
using System.Collections.Generic;

namespace Ktpt.CourseOrder.Models;

public partial class VwOrdersSummary
{
    public int OrderId { get; set; }

    public DateOnly OrderDate { get; set; }

    public int ClientId { get; set; }

    public string ClientFullName { get; set; } = null!;

    public decimal? TotalAmount { get; set; }
}
