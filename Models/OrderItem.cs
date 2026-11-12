using System;
using System.Collections.Generic;

namespace Ktpt.CourseOrder.Models;

public partial class OrderItem
{
    public int ItemId { get; set; }

    public int OrderId { get; set; }

    public int SlotId { get; set; }

    public int Quantity { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual Slot Slot { get; set; } = null!;
}
