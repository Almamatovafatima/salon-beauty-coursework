// Правила расчёта. Здесь нет ни базы данных, ни окон — только арифметика.
// Поэтому эти методы легко проверить отдельной программой (tests/RulesChecks).
public static class PriceRules
{
    public static (DateOnly start, DateOnly end) PreviousMonth(DateOnly today)
    {
        var firstDayCurrentMonth = new DateOnly(today.Year, today.Month, 1);
        var lastDayPreviousMonth = firstDayCurrentMonth.AddDays(-1);
        var firstDayPreviousMonth = new DateOnly(lastDayPreviousMonth.Year, lastDayPreviousMonth.Month, 1);
        return (firstDayPreviousMonth, lastDayPreviousMonth);
    }

    // Правило 1: Скидка 20% при сумме заказов > 5000 руб.
    public static decimal Discount(decimal basePrice, bool hasExpensiveOrders)
    {
        // Если прошлые заказы были > 5000, применяем скидку 20% (цена * 0.8)
        decimal price = hasExpensiveOrders ? basePrice * 0.8m : basePrice;

        // Округляем до копеек (2 знака)
        return decimal.Round(price, 2, MidpointRounding.AwayFromZero);
    }

    // Правило 2: Зависимость цены от спроса (наценка 15%, если > 3 клиентов) 
    public static decimal DynamicPrice(decimal basePrice, int bookedCount)
    {
        // Если записано более 3 клиентов, повышаем цену на 15% (умножаем на 1.15)
        decimal price = bookedCount > 3 ? basePrice * 1.15m : basePrice;

        return decimal.Round(price, 2, MidpointRounding.AwayFromZero);
    }


    // Текст о местах: больше 5 — «много», иначе «мало».
    public static string Availability(int seats) => seats > 5 ? "много" : "мало";

    // Красная подсветка карточки: мест 3 или меньше.
    public static bool LowStock(int seats) => seats <= 3;
}