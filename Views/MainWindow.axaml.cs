using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Ktpt.CourseOrder.Services;

namespace Ktpt.CourseOrder.Views;
public partial class MainWindow : Window
{
    public static MainWindow Instance { get; private set; } = null!;

    // История открытых страниц для кнопки «Назад» (последняя — сверху).
    private readonly Stack<UserControl> history = new();

    public MainWindow()
    {
        InitializeComponent();                            // создаёт элементы из .axaml
        Instance = this;                                  // запоминаем окно
        OpenPage(new LoginView(), clearHistory: true);    // первая страница — вход
    }
    // Открыть страницу. clearHistory: true — начать историю заново (вход, выход).
    public void OpenPage(UserControl page, bool clearHistory = false)
    {
        if (clearHistory)
        {
            history.Clear();
        }
        else
        {
            // Текущую страницу кладём в историю, чтобы к ней можно было вернуться.
            UserControl? current = PageHost.Content as UserControl;
            if (current != null)
                history.Push(current);
        }
        ShowPage(page);
    }

    // Показать страницу и обновить шапку и кнопки.
    private void ShowPage(UserControl page)
    {
        PageHost.Content = page;                       // подставить страницу в окно
        Title = "КТПТ — " + page.Tag;                  // Tag страницы = её заголовок
        MessageText.Text = "";                         // убрать старое сообщение
        UserText.Text = Session.Caption;               // ФИО и роль (или «Гость»)
        OrdersButton.IsVisible = Session.IsStaff;      // «Заказы» — только сотрудникам
        BackButton.IsEnabled = history.Count > 0;      // «Назад», если есть куда
    }
    // Обычное сообщение (чёрным цветом).
    public void ShowMessage(string text)
    {
        MessageText.Foreground = Brushes.Black;
        MessageText.Text = text;
    }

    // Сообщение об ошибке (тёмно-красным цветом).
    public void ShowError(Exception error)
    {
        System.Diagnostics.Trace.WriteLine(error); // подробности — в окно Output
        MessageText.Foreground = Brushes.DarkRed;
        MessageText.Text = UiSupport.ErrorText(error);
        this.Title = "ОШИБКА: " + (error.InnerException != null ? error.InnerException.Message : error.Message);
    }
    // ---------- Обработчики кнопок навигации ----------
    private void BackClick(object? sender, RoutedEventArgs e)
    {
        if (history.Count > 0)
            ShowPage(history.Pop()); // Pop — достать последнюю страницу из истории
    }
    private void CatalogClick(object? sender, RoutedEventArgs e)
    {
        OpenPage(new CatalogView(), clearHistory: true);
    }
    private void OrdersClick(object? sender, RoutedEventArgs e)
    {
        OpenPage(new OrderView());
    }
    private void LogoutClick(object? sender, RoutedEventArgs e)
    {
        Session.Logout();                                // снова гость
        OpenPage(new LoginView(), clearHistory: true);   // история очищается
    }

}