using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;
using Ktpt.CourseOrder.Models;
using Ktpt.CourseOrder.Services;

namespace Ktpt.CourseOrder.Views;

public partial class LoginView : UserControl
{
    public LoginView()
    {
        InitializeComponent();
        Loaded += async (s, e) => await LoadUsersAsync();
    }
    private async Task LoadUsersAsync()
    {
        try
        {
            UsersBox.ItemsSource = await Session.GetUsersAsync();
        }
        catch (Exception ex)
        {
            MainWindow.Instance.ShowError(ex); // например, сервер недоступен
        }
    }
    // Кнопка «Обновить список».
    private async void ReloadClick(object? sender, RoutedEventArgs e)
    {
        await LoadUsersAsync();
    }
    // Кнопка «Войти».
    private async void LoginClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            // SelectedItem — выбранный элемент списка; as User — «считать его User».
            User? user = UsersBox.SelectedItem as User;
            if (user == null)
                throw new AppException("Выберите логин из списка.");
            await Session.LoginAsync(user.UserId);
            MainWindow.Instance.OpenPage(new CatalogView(), clearHistory: true);
        }
        catch (Exception ex)
        {
            MainWindow.Instance.ShowError(ex);
        }
    }
    // Кнопка «Войти как гость».
    private void GuestClick(object? sender, RoutedEventArgs e)
    {
        Session.Logout();
        MainWindow.Instance.OpenPage(new CatalogView(), clearHistory: true);
    }
}