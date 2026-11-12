using Avalonia.Controls;
using Avalonia.Interactivity;
using Ktpt.CourseOrder.Data;
using Ktpt.CourseOrder.Models;
using Ktpt.CourseOrder.Services;
using Microsoft.EntityFrameworkCore;

namespace Ktpt.CourseOrder.Views;

public partial class ServicesView : UserControl
{
    // Номер открытой услуги. Задаётся при создании страницы:
    //     new CourseView { CourseId = 5 }
    public int ServicesId { get; set; }
    private decimal shownPrice;
    public ServicesView()
    {
        InitializeComponent();
        Loaded += async (s, e) => await LoadServiceAsync();
    }
    // Загрузить услугу, её потоки и (для сотрудника) список клиентов.
    private async Task LoadServiceAsync() // Переименовали метод для логики
    {
        try
        {
            SaveButton.IsEnabled = false; // Пока данные не загружены — не оформляем
            using var db = Db.Create();

            // Ищем услугу по ID и сразу подгружаем связанные слоты (расписание)
            // Замените Service и Slots на точные названия классов из папки Models!
            Service? service = await db.Services
                .Include(s => s.Slots)   // вместе с расписанием (слотами)
                .AsNoTracking()
                .SingleOrDefaultAsync(s => s.ServiceId == ServicesId);

            if (service == null)
                throw new AppException("Услуга не найдена. Вернитесь в каталог.");

            // Заполняем элементы страницы.
            // Убедитесь, что в XAML у вас есть элементы с такими именами, или поменяйте их тут.
            NameText.Text = service.ServiceName;

            // Если у вас нет поля Description в базе, можно вывести категорию или длительность
            // DescriptionText.Text = service.Description; 

            CourseImage.Source = UiSupport.Picture(service.image_path);

            // Получаем цену
            shownPrice = await CatalogService.GetPriceAsync(db, service);
            PriceText.Text = $"Цена услуги: {shownPrice:N2} руб.";

            // Отбираем только те слоты, где есть свободные места (max_seats больше, чем уже записано)
            // В вашей БД есть поле max_seats. Если логика подсчета свободных мест сложнее, 
            // то нужно использовать View (vw_slots_availability), но пока берем простой вариант.
            List<Slot> availableSlots = service.Slots
                .Where(sl => sl.MaxSeats > 0) // Или другая ваша логика проверки мест
                .OrderBy(sl => sl.SlotDate)
                .ThenBy(sl => sl.SlotTime)
                .ToList();

            SlotBox.ItemsSource = availableSlots;
            SlotBox.SelectedIndex = availableSlots.Count > 0 ? 0 : -1; // -1 = ничего не выбрано

            // Выбор клиента — только для администратора (сотрудника).
            ClientPanel.IsVisible = Session.IsStaff;
            if (Session.IsStaff)
            {
                List<User> users = await Session.GetUsersAsync();
                ClientBox.ItemsSource = users;
                // По умолчанию выбран сам сотрудник.
                ClientBox.SelectedItem = users.FirstOrDefault(
                    u => u.UserId == Session.CurrentUser!.UserId);
            }

            SaveButton.IsEnabled = availableSlots.Count > 0;
            if (availableSlots.Count == 0)
                MainWindow.Instance.ShowMessage("На эту услугу пока нет свободных слотов.");
        }
        catch (Exception ex)
        {
            MainWindow.Instance.ShowError(ex);
        }
    }
    // Кнопка «Оформить».
    private async void SaveClick(object? sender, RoutedEventArgs e)
    {
        SaveButton.IsEnabled = false; // защита от двойного щелчка
        try
        {
            // 1) Слот (расписание). Выбираем, на какое время и к какому мастеру записываемся.
            // Замените Slot на точное название вашего класса из папки Models!
            Slot? selectedSlot = SlotBox.SelectedItem as Slot;
            if (selectedSlot == null)
                throw new AppException("Выберите свободное время (слот) для записи.");

            // 2) Количество: для салона это обычно 1 место (клиент), 
            // но если это групповые услуги (например, мастер-класс), логика остается.
            if (!int.TryParse(QuantityBox.Text, out int quantity) || quantity <= 0)
                throw new AppException("Количество мест должно быть целым числом больше нуля.");

            // 3) Клиент: если зашел клиент — записываем его самого. 
            // Если администратор — он должен выбрать клиента из списка.
            int clientId;
            if (Session.IsStaff) // Если зашел сотрудник (админ/мастер)
            {
                User? client = ClientBox.SelectedItem as User;
                if (client == null)
                    throw new AppException("Выберите клиента для оформления записи.");
                clientId = client.UserId;
            }
            else // Если зашел сам клиент
            {
                clientId = Session.CurrentUser!.UserId;
            }

            // 4) Сохранение. Вызываем метод из сервиса, который проверит свободные места 
            // и запишет данные в таблицы orders и order_items.
            // Передаем ID слота, количество и ID клиента.
            int orderId = await OrderService.CreateOrderAsync(
                selectedSlot.SlotId, quantity, clientId, shownPrice);

            // 5) Успех: возвращаемся в каталог услуг и показываем сообщение.
            MainWindow.Instance.OpenPage(new CatalogView(), clearHistory: true);
            MainWindow.Instance.ShowMessage($"Запись № {orderId} успешно оформлена. Свободные места обновлены.");
        }
        catch (Exception ex)
        {
            // Если произошла ошибка (например, места закончились или база недоступна) — показываем её.
            MainWindow.Instance.ShowError(ex);
            SaveButton.IsEnabled = true; // Разрешаем нажать кнопку снова после исправления
        }
    }
    private async void ReloadClick(object? sender, RoutedEventArgs e)
    {
        // Здесь мы просто заново вызываем метод загрузки данных, 
        // который обновит информацию об услуге и список доступных слотов.
        await LoadServiceAsync();
    }

}