using System;
using System.Collections.Generic;

namespace Ktpt.CourseOrder.Models;

public partial class Slot
{
    public int SlotId { get; set; }

    public DateOnly SlotDate { get; set; }

    public TimeOnly SlotTime { get; set; }

    public int MaxSeats { get; set; }

    public int ServiceId { get; set; }

    public int MasterId { get; set; }

    public virtual User Master { get; set; } = null!;

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual Service Service { get; set; } = null!;
    public override string ToString()
    {
        // Форматируем дату и время, чтобы было красиво (например: 10.11.2026 14:00)
        return $"{SlotDate:dd.MM.yyyy} в {SlotTime:hh\\:mm}";
    }
}
