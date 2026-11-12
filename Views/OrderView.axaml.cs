using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;
using Ktpt.CourseOrder.Models;
using Ktpt.CourseOrder.Services;


namespace Ktpt.CourseOrder.Views;

public partial class OrderView : UserControl
{
    public OrderView()
    {
        InitializeComponent();
        Loaded += async (s, e) => await LoadOrdersAsync();
    }
    // Загрузить все заказы.
    private async Task LoadOrdersAsync()
    {
        try
        {
            OrdersList.ItemsSource = await OrderService.GetOrdersAsync();
        }
        catch (Exception ex)
        {
            MainWindow.Instance.ShowError(ex);
        }
    }
    // Кнопка «Обновить».
    private async void ReloadClick(object? sender, RoutedEventArgs e)
    {
        await LoadOrdersAsync();
    }
    // Кнопка «Новый заказ»: переходим в каталог, там выбираем услугу и клиента.
    private void NewClick(object? sender, RoutedEventArgs e)
    {
        MainWindow.Instance.OpenPage(new CatalogView());
    }
    // Кнопка «Состав заказа».
    private void OpenClick(object? sender, RoutedEventArgs e)
    {
        Order? order = OrdersList.SelectedItem as Order;
        if (order == null)
        {
            MainWindow.Instance.ShowMessage("Сначала выберите заказ в списке.");
            return;
        }
        MainWindow.Instance.OpenPage(new OrderItemView { OrderId = order.OrderId });
    }
}