using Ktpt.CourseOrder.Data;
using Ktpt.CourseOrder.Models;
using Ktpt.CourseOrder.Services;
using Microsoft.EntityFrameworkCore;
public static class OrderService
{
    // --- 1. ОФОРМЛЕНИЕ ЗАКАЗА ---
    public static async Task<int> CreateOrderAsync(int slotId, int quantity, int clientId, decimal shownPrice)
    {
        if (Session.CurrentUser == null)
            throw new AppException("Войдите в систему.");

        if (quantity <= 0)
            throw new AppException("Количество должно быть больше нуля.");

        if (!Session.IsStaff) clientId = Session.CurrentUser.UserId;

        using var db = Db.Create();
        using var transaction = await db.Database.BeginTransactionAsync();

        try
        {
            // 1. Получаем слот
            var slot = await db.Slots.Include(s => s.Service).AsNoTracking()
                .SingleOrDefaultAsync(s => s.SlotId == slotId);

            if (slot == null) throw new AppException("Слот не найден.");
            if (slot.Service.base_price != shownPrice) throw new AppException("Цена изменилась. Обновите.");

            // 2. СТРОГАЯ ПРОВЕРКА. Если мест меньше, чем нужно — ОШИБКА (до базы!)
            if (slot.MaxSeats < quantity)
                throw new AppException($"На это время осталось только {slot.MaxSeats} мест. Выберите другое.");

            // 3. Обновляем количество мест
            slot.MaxSeats -= quantity;
            db.Slots.Update(slot); // EF Core сам сделает UPDATE
            await db.SaveChangesAsync();

            // 4. Создаем заказ
            var order = new Order { OrderDate = DateOnly.FromDateTime(DateTime.Today), ClientId = clientId };
            db.Orders.Add(order);
            await db.SaveChangesAsync();

            // 5. Создаем позицию
            var orderItem = new OrderItem { OrderId = order.OrderId, SlotId = slotId, Quantity = quantity };
            db.OrderItems.Add(orderItem);
            await db.SaveChangesAsync();

            await transaction.CommitAsync();
            return order.OrderId;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
    public static async Task<List<Order>> GetOrdersAsync()
    {
         Session.RequireStaff(); // Проверка прав (если у вас есть класс Session)

        using var db = Db.Create();

        return await db.Orders
            .Include(o => o.Client)                 // ФИО клиента
            .OrderByDescending(o => o.OrderDate)    // новые сверху
            .ThenByDescending(o => o.OrderId)
            .AsNoTracking()
            .ToListAsync();
    }

    // --- 3. ОДИН ЗАКАЗ СО ВСЕМ СОСТАВОМ ---
    public static async Task<Order?> GetOrderAsync(int orderId)
    {
         Session.RequireStaff(); // Проверка прав

        using var db = Db.Create();

        return await db.Orders
            .Include(o => o.Client)
            .Include(o => o.OrderItems)             // позиции заказа
                .ThenInclude(i => i.Slot)           // в позиции — слот (время/мастер)
                    .ThenInclude(s => s.Service)    // в слоте — услуга
            .AsNoTracking()
            .SingleOrDefaultAsync(o => o.OrderId == orderId);
    }
    public static async Task ChangeDateAsync(int orderId, DateOnly newDate)
    {
         Session.RequireAdmin(); // Проверка прав (раскомментируйте, если есть класс Session)

        using var db = Db.Create();

        Order? order = await db.Orders.FindAsync(orderId); // найти по первичному ключу

        if (order == null)
            throw new AppException("Заказ уже удалён. Обновите список.");

        order.OrderDate = newDate; // меняем свойство объекта...

        await db.SaveChangesAsync(); // ...EF сам отправит UPDATE в базу
    }
    public static async Task DeleteAsync(int orderId, int? itemId)
    {
         Session.RequireStaff(); // Проверки прав (раскомментируйте, если есть Session)
        if (itemId == null) Session.RequireAdmin();

        using var db = Db.Create();
        using var transaction = await db.Database.BeginTransactionAsync();

        // Какие позиции удаляем: одну выбранную или все позиции заказа.
        List<OrderItem> items = await db.OrderItems
            .Where(i => i.OrderId == orderId && (itemId == null || i.ItemId == itemId))
            .ToListAsync();

        if (items.Count == 0)
            throw new AppException("Эти данные уже удалены. Обновите список.");

        // Возвращаем места: прибавляем количество к текущему значению в базе.
        foreach (OrderItem item in items)
        {
            await db.Slots
                .Where(v => v.SlotId == item.SlotId)
                .ExecuteUpdateAsync(s => s.SetProperty(
                    v => v.MaxSeats,
                    v => v.MaxSeats + item.Quantity));
        }

        db.OrderItems.RemoveRange(items); // пометить позиции «удалить»
        await db.SaveChangesAsync();      // DELETE в базе

        // Если в заказе не осталось позиций — удаляем и сам заказ.
        bool hasItems = await db.OrderItems.AnyAsync(i => i.OrderId == orderId);
        if (!hasItems)
        {
            await db.Orders.Where(o => o.OrderId == orderId).ExecuteDeleteAsync();
        }

        await transaction.CommitAsync(); // возврат мест и удаление — вместе
    }
}