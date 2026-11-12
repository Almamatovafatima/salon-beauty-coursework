using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Kptp.CourseOrder.Data;
using Ktpt.CourseOrder.Services;

namespace Ktpt.CourseOrder.Views;

public partial class CatalogView : UserControl
{
    private List<ServiceCard> allCards = new();
    public CatalogView()
    {
        InitializeComponent();
        SortBox.SelectedIndex = 0; // по умолчанию — «Цена по возрастанию»
        Loaded += async (s, e) => await LoadCardsAsync();

    }
    private async Task LoadCardsAsync()
    {
        try
        {
            // Запоминаем выбранную категорию, чтобы после обновления она осталась.
            string? oldCategory = CategoryBox.SelectedItem as string;

            allCards = await CatalogService.LoadCardsAsync();

            // Категории: первая строка «Все категории», дальше — без повторов, по алфавиту.
            var categories = new List<string> { "Все категории" };
            categories.AddRange(allCards.Select(c => c.Category).Distinct().OrderBy(c => c));
            CategoryBox.ItemsSource = categories;

            if (oldCategory != null && categories.Contains(oldCategory))
                CategoryBox.SelectedItem = oldCategory;
            else
                CategoryBox.SelectedIndex = 0;

            ApplyFilters();
        }
        catch (Exception ex)
        {
            MainWindow.Instance.ShowError(ex);
        }
    }
    private void ApplyFilters()
    {
        string text = (SearchBox.Text ?? "").Trim();
        // 1) Поиск: название ИЛИ код содержит текст (регистр букв не важен).
        IEnumerable<ServiceCard> rows = allCards.Where(c =>
            c.Name.Contains(text, StringComparison.OrdinalIgnoreCase));
        // 2) Категория: индекс 0 — «Все категории», тогда не фильтруем.
        if (CategoryBox.SelectedIndex > 0)
        {
            string category = (string)CategoryBox.SelectedItem!;
            rows = rows.Where(c => c.Category == category);
        }
        // 3) Сортировка по цене: индекс 1 — по убыванию, иначе по возрастанию.
        if (SortBox.SelectedIndex == 1)
            rows = rows.OrderByDescending(c => c.Price);
        else
            rows = rows.OrderBy(c => c.Price);
        // 4) Показать результат.
        List<ServiceCard> result = rows.ToList();
        CardsList.ItemsSource = result;
        CountText.Text = $"Найдено: {result.Count} из {allCards.Count}";
    }
    // Изменился текст поиска.
    private void SearchChanged(object? sender, TextChangedEventArgs e)
    {
        ApplyFilters();
    }
    // Изменилась категория или сортировка.
    private void FilterChanged(object? sender, SelectionChangedEventArgs e)
    {
        ApplyFilters();
    }
    // Кнопка «Обновить».
    private async void ReloadClick(object? sender, RoutedEventArgs e)
    {
        await LoadCardsAsync();
    }
    // Пользователь выбрал карточку — открываем страницу услуги.
    private void CardSelected(object? sender, SelectionChangedEventArgs e)
    {
        // Проверяем, что пользователь действительно выбрал карточку
        if (CardsList.SelectedItem is ServiceCard selectedCard)
        {
            // Создаем окно оформления и передаем туда ID выбранной услуги
            var view = new ServicesView { ServicesId = selectedCard.Id };

            // Открываем это окно в главном окне
            MainWindow.Instance.OpenPage(view);
        }
    }

}
    
