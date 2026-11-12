using Kptp.CourseOrder.Data;
using Microsoft.EntityFrameworkCore;

namespace Ktpt.CourseOrder.Data;

public static class Db
{
    public static SalonServices Create()
    {
        // Получаем папку, откуда запущена программа
        string baseDir = AppContext.BaseDirectory;
        string path = Path.Combine(baseDir, "connection.txt");

        // Если файла нет - выводим понятную ошибку с ПУТЕМ
        if (!File.Exists(path))
        {
            throw new Exception($"ОШИБКА: Файл не найден по пути: {path}. Проверьте, что он лежит в папке bin/Debug/net10.0/");
        }

        string connection = File.ReadAllText(path).Trim();

        var options = new DbContextOptionsBuilder<SalonServices>()
            .UseSqlServer(connection)
            .Options;

        return new SalonServices(options);
    }
}