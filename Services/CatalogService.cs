using Kptp.CourseOrder.Data;
using Ktpt.CourseOrder.Data;
using Ktpt.CourseOrder.Models;
using Ktpt.CourseOrder.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client.NativeInterop;

// Чтение данных для каталога и расчёт текущей цены услуги.
    public static class CatalogService
    {
        //  Текущая цена услуги (с учетом бизнес-правил) ===
        public static async Task<decimal> GetPriceAsync(SalonServices db, Service service)
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.Today);
            var (start, end) = PriceRules.PreviousMonth(today); // границы прошлого месяца

            // 1. Правило скидки: Считаем общую сумму заказов клиента за прошлый месяц.
            decimal pastSum = await db.Orders
                .Where(o => o.OrderDate >= start && o.OrderDate <= end)
                .SelectMany(o => o.OrderItems)
                .SumAsync(i => i.Quantity * i.Slot.Service.base_price);

            bool hasExpensiveOrders = pastSum > 5000; // Условие вашей скидки

            // 2. Правило спроса: Считаем, сколько всего человек записано на эту услугу
            int bookedCount = await db.OrderItems
                .Where(i => i.Slot.ServiceId == service.ServiceId)
                .SumAsync(i => i.Quantity);

            // Вызываем правила из PriceRules
            decimal priceWithDiscount = PriceRules.Discount(service.base_price, hasExpensiveOrders);
            decimal finalPrice = PriceRules.DynamicPrice(priceWithDiscount, bookedCount);

            return service.base_price;
        }

        //  3: Карточки каталога ===
        public static async Task<List<ServiceCard>> LoadCardsAsync()
        {
            using var db = Db.Create();

            // Загружаем услуги вместе со связанными данными
            List<Service> services = await db.Services
                .Include(s => s.Category) // название категории
                .Include(s => s.Slots)    // слоты (чтобы сложить места)
                .AsNoTracking()
                .ToListAsync();

            var cards = new List<ServiceCard>();

            foreach (Service s in services)
            {
                // Считаем свободные места по всем слотам этой услуги (предполагаем, что max_seats = свободные места)
                int seats = s.Slots.Sum(v => v.MaxSeats);

                // Получаем цену с учетом правил
                decimal price = await GetPriceAsync(db, s);

                // Создаём карточку и заполняем её свойства
                cards.Add(new ServiceCard
                {
                    Id = s.ServiceId,
                    Name = s.ServiceName,
                    Category = s.Category.CategoryName,
                    MasterName = "Разные мастера", // Или можно вытащить имя первого мастера
                    Duration = $"{s.DurationMinutes} мин",
                    ImagePath = string.IsNullOrWhiteSpace(s.image_path)
                    ? "avares://Ktpt.CourseOrder/Assets/picture.jpg"
                    : $"avares://Kptp.CourseOrder/Assets/Images/{s.image_path}",
                    Seats = seats,
                    SeatsText = "Мест: " + PriceRules.Availability(seats),
                    Price = price,
                    PriceText = $"{price:N2} руб.",

                    Background = UiSupport.Brush(PriceRules.LowStock(seats)
                        ? "LowStockBrush"
                        : "NormalBrush"),

                    Image = UiSupport.Picture(s.image_path), // Загружаем картинку
                   
                });
            }

            return cards;
        }
    }
