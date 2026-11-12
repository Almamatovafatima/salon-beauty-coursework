using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;
using Ktpt.CourseOrder.Models;
using Ktpt.CourseOrder.Services;

namespace Ktpt.CourseOrder.Views;

public partial class OrderItemView : UserControl
{
    public int OrderId { get; set; }
    private int? itemToDelete;
    public OrderItemView()
    {
        InitializeComponent();
        Loaded += async (s, e) => await LoadOrderAsync();
    }
    private async Task LoadOrderAsync()
    {
        try
        {
            ConfirmPanel.IsVisible = false;
            Order? order = await OrderService.GetOrderAsync(OrderId);
            if (order == null)
            {
                // Заказа больше нет (удалили весь заказ или последнюю позицию).
                MainWindow.Instance.OpenPage(new OrderView(), clearHistory: true);
                MainWindow.Instance.ShowMessage("Заказ удалён, места возвращены.");
                return;
            }
            HeaderText.Text = $"Заказ № {order.OrderId}";
            ClientText.Text = "Клиент: " + order.Client.FullName;
            DateText.Text = "Дата: " + order.OrderDate.ToString("dd.MM.yyyy");
            TotalText.Text = $"Итого: {order.OrderItems.Sum(i => (i.Slot?.Service?.base_price ?? 0) * i.Quantity):N2} руб.";
            ItemsList.ItemsSource = order.OrderItems.ToList();
            // Панель администратора видна только администратору.
            AdminPanel.IsVisible = Session.IsAdmin;
            // CalendarDatePicker работает с DateTime, а в БД — DateOnly: переводим.
            DateBox.SelectedDate = order.OrderDate.ToDateTime(TimeOnly.MinValue);
        }
        catch (Exception ex)
        {
            MainWindow.Instance.ShowError(ex);
        }
    }
    // Кнопка «Обновить».
    private async void ReloadClick(object? sender, RoutedEventArgs e)
    {
        await LoadOrderAsync();
    }

    // Кнопка «Сохранить дату» (администратор).
    private async void SaveDateClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (DateBox.SelectedDate == null)
                throw new AppException("Выберите дату.");

            DateOnly date = DateOnly.FromDateTime(DateBox.SelectedDate.Value);
            await OrderService.ChangeDateAsync(OrderId, date);
            await LoadOrderAsync();
            MainWindow.Instance.ShowMessage("Дата сохранена. Цены позиций не изменились.");
        }
        catch (Exception ex)
        {
            MainWindow.Instance.ShowError(ex);
        }
    }
    // Кнопка «Удалить весь заказ»: только показываем вопрос.
    private void DeleteOrderClick(object? sender, RoutedEventArgs e)
    {
        itemToDelete = null;
        ConfirmText.Text = "Удалить весь заказ? Все его места вернутся в потоки.";
        ConfirmPanel.IsVisible = true;
    }
    // Кнопка «Удалить выбранную позицию»: только показываем вопрос.
    private void DeleteItemClick(object? sender, RoutedEventArgs e)
    {
        OrderItem? item = ItemsList.SelectedItem as OrderItem;
        if (item == null)
        {
            MainWindow.Instance.ShowMessage("Сначала выберите позицию в списке.");
            return;
        }
        itemToDelete = item.ItemId;
        ConfirmText.Text = "Удалить позицию? Её места вернутся в поток. " +
                           "Если позиция последняя, удалится и весь заказ.";
        ConfirmPanel.IsVisible = true;
    }
    // Кнопка «Отмена»: скрыть вопрос, ничего не удалять.
    private void CancelClick(object? sender, RoutedEventArgs e)
    {
        ConfirmPanel.IsVisible = false;
    }
    // Кнопка «Да, удалить».
    private async void ConfirmClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            await OrderService.DeleteAsync(OrderId, itemToDelete);
            MainWindow.Instance.ShowMessage("Удаление выполнено, места возвращены.");
            await LoadOrderAsync(); // перечитать заказ (или уйти к списку, если его нет)
        }
        catch (Exception ex)
        {
            MainWindow.Instance.ShowError(ex);
        }
    }
}
