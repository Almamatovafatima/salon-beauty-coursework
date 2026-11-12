using Avalonia.Media;
using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Text;

public class ServiceCard
{
    public int Id { get; set; }                  // service_id - чтобы открыть услугу
    public string Name { get; set; } = "";       // название услуги
    public string Category { get; set; } = "";   // название категории (например, "Парикмахерские")
    public string MasterName { get; set; } = "";
    public string Duration { get; set; } = "";   // длительность (например, "60 мин")
    public int Seats { get; set; }
    public string SeatsText { get; set; } = "";  // Текст: "много" или "мало"
    public decimal Price { get; set; }
    public string PriceText { get; set; } = "";  // Текст цены с рублями (например, "1500,00 руб.")
    public IBrush? Background { get; set; }      // Цвет фона (красный, если мест мало)
    public Bitmap? Image { get; set; }  // Сама картинка услуги
    public string? ImagePath { get; set; }
}
