using Ktpt.CourseOrder.Data;
using Ktpt.CourseOrder.Models;
using Ktpt.CourseOrder.Services;
using Microsoft.EntityFrameworkCore;

// Одна карточка каталога: всё, что нужно показать на экране для одной услуги.
// Разметка CatalogView.axaml берёт значения отсюда через {Binding ИмяСвойства}.
// Значения заполняет метод CatalogService.LoadCardsAsync.
// Session («сеанс») помнит, кто сейчас работает с программой.
// Класс static: он один на всю программу, пишем просто Session.CurrentUser.
public static class Session
{
    // Текущий пользователь. null означает «гость» (никто не вошёл).
    // private set — изменить значение можно только методами этого класса.
    public static User? CurrentUser { get; private set; }

    // Название роли текущего пользователя; для гостя — пустая строка.
    public static string RoleName => CurrentUser?.Role.RoleName ?? "";

    // Уровни доступа.
    // АДАПТАЦИЯ: названия ролей пишите ТОЧНО как в таблице roles вашей БД.
    public static bool IsGuest => CurrentUser == null;
    public static bool IsStaff => RoleName == "Мастер" || RoleName == "Администратор";
    public static bool IsAdmin => RoleName == "Администратор";

    // Подпись в шапке окна: «Иванов Иван Иванович · Студент» или «Гость».
    public static string Caption =>
        CurrentUser == null ? "Гость" : $"{CurrentUser.FullName} · {RoleName}";
    // Все пользователи для выпадающего списка (вместе с ролями).
    public static async Task<List<User>> GetUsersAsync()
    {
        using var db = Db.Create();
        return await db.Users
            .Include(u => u.Role)   // сразу загрузить роль каждого пользователя
            .OrderBy(u => u.Login)  // по алфавиту логинов
            .AsNoTracking()         // только читаем, изменять не будем
            .ToListAsync();         // выполнить запрос и получить список
    }

    // Вход: роль заново читается из БД, а не берётся из подписи в списке.
    public static async Task LoginAsync(int userId)
    {
        using var db = Db.Create();
        CurrentUser = await db.Users
            .Include(u => u.Role)
            .AsNoTracking()
            .SingleAsync(u => u.UserId == userId);
    }

    // Выход: снова гость.
    public static void Logout()
    {
        CurrentUser = null;
    }
    // Проверки прав. Вызываются в сервисах ПЕРЕД изменением данных.
    // Если прав нет — выбрасываем ошибку, и операция не выполняется.
    public static void RequireStaff()
    {
        if (!IsStaff)
            throw new AppException("Недостаточно прав: нужна роль администратора.");
    }

    public static void RequireAdmin()
    {
        if (!IsAdmin)
            throw new AppException("Недостаточно прав: нужна роль администратора.");
    }
}