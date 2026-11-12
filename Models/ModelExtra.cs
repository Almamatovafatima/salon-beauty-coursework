using System;
using System.Collections.Generic;
using System.Text;

namespace Ktpt.CourseOrder.Models;

public partial class User
{
    // ФИО одной строкой. Trim убирает пробел в конце, если отчества нет.
    public string FullName => $"{LastName} {FirstName} {MiddleName}".Trim();

    // ToString решает, как объект выглядит в ComboBox и ListBox.
    public override string ToString() => $"{Login} — {FullName}";
}


public partial class Order
{
    // Так заказ выглядит в списке заказов: номер, дата, ФИО клиента.
    public override string ToString() =>
        $"Заказ № {OrderId} · {OrderDate:dd.MM.yyyy} · {Client?.FullName}";
}
public partial class OrderItem
{
    // Этот метод переопределяет стандартный вывод и делает его красивым
    public override string ToString()
    {
        // Берем название услуги из связанного слота (если оно подгружено), количество и цену
        string serviceName = Slot?.Service?.ServiceName ?? "Услуга";

        // Считаем итоговую цену (если цены нет в OrderItem, берем из Service)
        decimal price = (Slot?.Service?.base_price ?? 0) * Quantity;

        return $"{serviceName} — {Quantity} шт. на сумму {price:N2} руб.";
    }
}