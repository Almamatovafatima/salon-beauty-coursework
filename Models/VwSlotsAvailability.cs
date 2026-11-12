using System;
using System.Collections.Generic;

namespace Ktpt.CourseOrder.Models;

public partial class VwSlotsAvailability
{
    public int SlotId { get; set; }

    public DateOnly SlotDate { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly? EndTime { get; set; }

    public string ServiceName { get; set; } = null!;

    public decimal BasePrice { get; set; }

    public int MasterId { get; set; }

    public int MaxSeats { get; set; }

    public int? AvailableSeats { get; set; }
}
