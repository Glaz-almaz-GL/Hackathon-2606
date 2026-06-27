using OSGeo.GDAL;
using Plotly.NET;
using Plotly.NET.LayoutObjects;
using System.Globalization;
using System.Numerics;
using static Plotly.NET.StyleParam;

internal class Program
{
    public static void Main()
    {
        // Критически важно: устанавливаем инвариантную культуру ДО любых операций с числами
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

        Gdal.AllRegister();

        string tiffPath = @"F:\Рабочий стол\output_hh.tif";
        string outputPath = @"F:\Рабочий стол\output_hh.txt";
        string htmlPath = @"F:\Рабочий стол\output_hh.html";

        Console.WriteLine("Начало извлечения точек...");
        var vectors = Extract3DPoints(tiffPath);
        Console.WriteLine($"Извлечено {vectors.Length} точек.");

        Console.WriteLine("Запись в файл...");
        WritePointsToFile(vectors, outputPath);
        Console.WriteLine("Запись завершена.");

        Console.WriteLine("Создание визуализации...");
        //GenerateCubePoints(@"F:\Рабочий стол\cube_points.txt", 4, 1);
        VisualizeAsMesh(outputPath, htmlPath);
        Console.WriteLine($"Визуализация сохранена в: {htmlPath}");
    }

    /// <summary>
    /// Эффективная запись точек в файл с использованием StreamWriter
    /// </summary>
    private static void WritePointsToFile(Vector3[] points, string outputPath)
    {
        // Используем StreamWriter с буфером - это в 100-1000 раз быстрее
        using (var writer = new StreamWriter(outputPath, append: false))
        {
            writer.WriteLine("X;Y;Z");

            foreach (var point in points)
            {
                // Формат F6 даёт 6 знаков после запятой (достаточно для GPS-координат)
                writer.WriteLine($"{point.X:F6};{point.Y:F6};{point.Z:F3}");
            }
        }
    }

    public static Vector3[] Extract3DPoints(string path)
    {
        using (Dataset ds = Gdal.Open(path, Access.GA_ReadOnly))
        {
            if (ds == null)
                throw new Exception($"Не удалось открыть файл: {path}");

            int width = ds.RasterXSize;
            int height = ds.RasterYSize;

            double[] gt = new double[6];
            ds.GetGeoTransform(gt);

            using (Band band = ds.GetRasterBand(1))
            {
                // Получаем NoData значение
                band.GetNoDataValue(out double noDataValue, out int hasNoData);

                float[] zValues = new float[width * height];
                band.ReadRaster(0, 0, width, height, zValues, width, height, 0, 0);

                // Сначала считаем, сколько валидных точек (чтобы не создавать массив с пустотами)
                int validCount = 0;
                for (int i = 0; i < zValues.Length; i++)
                {
                    if (hasNoData == 1 && Math.Abs(zValues[i] - noDataValue) < 0.001)
                        continue;
                    validCount++;
                }

                Vector3[] points = new Vector3[validCount];
                int index = 0;

                for (int row = 0; row < height; row++)
                {
                    for (int col = 0; col < width; col++)
                    {
                        int bufferIndex = row * width + col;
                        float z = zValues[bufferIndex];

                        // Пропускаем NoData
                        if (hasNoData == 1 && Math.Abs(z - noDataValue) < 0.001)
                            continue;

                        // ВАЖНО: Вычисляем координаты ЦЕНТРА пикселя, а не угла
                        // Для этого добавляем 0.5 к col и row
                        double x = gt[0] + (col + 0.5) * gt[1] + (row + 0.5) * gt[2];
                        double y = gt[3] + (col + 0.5) * gt[4] + (row + 0.5) * gt[5];

                        points[index++] = new Vector3((float)x, (float)y, z);
                    }
                }

                return points;
            }
        }
    }

    /// <summary>
    /// Первый проход: сбор статистики и вычисление расстояния между точками
    /// </summary>
    private static (int uniqueXCount, int uniqueYCount,
                    double xMin, double xMax, double yMin, double yMax,
                    double distanceXMeters, double distanceYMeters)
        GetGridInfoAndDistance(string csvPath)
    {
        var uniqueX = new HashSet<double>();
        var uniqueY = new HashSet<double>();
        double xMin = double.MaxValue, xMax = double.MinValue;
        double yMin = double.MaxValue, yMax = double.MinValue;

        // Для вычисления расстояния между соседними точками
        double? prevX = null;
        double? prevY = null;
        double sumDeltaX = 0, sumDeltaY = 0;
        int countDeltaX = 0, countDeltaY = 0;

        using var fs = new FileStream(csvPath, FileMode.Open, FileAccess.Read, FileShare.Read,
            bufferSize: 1 << 16);
        using var reader = new StreamReader(fs);

        reader.ReadLine(); // Пропуск заголовка
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            var parts = line.Split(';');
            if (parts.Length < 3) continue;

            if (!double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out double x)) continue;
            if (!double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out double y)) continue;

            uniqueX.Add(x);
            uniqueY.Add(y);

            if (x < xMin) xMin = x;
            if (x > xMax) xMax = x;
            if (y < yMin) yMin = y;
            if (y > yMax) yMax = y;

            // Вычисляем разницу между соседними точками
            if (prevX.HasValue)
            {
                double deltaX = Math.Abs(x - prevX.Value);
                if (deltaX > 1e-12) // Пропускаем одинаковые X (новая строка)
                {
                    sumDeltaX += deltaX;
                    countDeltaX++;
                }
            }

            if (prevY.HasValue)
            {
                double deltaY = Math.Abs(y - prevY.Value);
                if (deltaY > 1e-12) // Пропускаем одинаковые Y (внутри строки)
                {
                    sumDeltaY += deltaY;
                    countDeltaY++;
                }
            }

            prevX = x;
            prevY = y;
        }

        // Среднее расстояние в градусах
        double avgDeltaXDeg = countDeltaX > 0 ? sumDeltaX / countDeltaX : 0;
        double avgDeltaYDeg = countDeltaY > 0 ? sumDeltaY / countDeltaY : 0;

        // Перевод в метры
        double avgLat = (yMin + yMax) / 2.0;
        double metersPerDegreeLon = 111320.0 * Math.Cos(avgLat * Math.PI / 180.0);
        double metersPerDegreeLat = 111320.0;

        double distanceXMeters = avgDeltaXDeg * metersPerDegreeLon;
        double distanceYMeters = avgDeltaYDeg * metersPerDegreeLat;

        return (uniqueX.Count, uniqueY.Count, xMin, xMax, yMin, yMax, distanceXMeters, distanceYMeters);
    }

    /// <summary>
    /// Второй проход: чтение точек с прореживанием через FileStream
    /// </summary>
    private static void ReadPointsWithStep(string csvPath, int step,
        List<double> x, List<double> y, List<double> z)
    {
        using var fs = new FileStream(csvPath, FileMode.Open, FileAccess.Read, FileShare.Read,
            bufferSize: 1 << 16);
        using var reader = new StreamReader(fs);

        reader.ReadLine(); // Пропуск заголовка
        string? line;

        int colIndex = 0;
        int rowIndex = 0;
        double lastY = double.NaN;

        while ((line = reader.ReadLine()) != null)
        {
            var parts = line.Split(';');
            if (parts.Length < 3) continue;

            if (!double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out double px)) continue;
            if (!double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out double py)) continue;
            if (!double.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out double pz)) continue;

            // Определяем переход на новую строку по смене Y
            if (!double.IsNaN(lastY) && Math.Abs(py - lastY) > 1e-12)
            {
                colIndex = 0;
                rowIndex++;
            }
            lastY = py;

            // Прореживание
            if (rowIndex % step == 0 && colIndex % step == 0)
            {
                x.Add(px);
                y.Add(py);
                z.Add(pz);
            }

            colIndex++;
        }
    }

    public static void VisualizeAsMesh(string csvPath, string outputHtmlPath)
    {
        // === Первый проход: сбор статистики и вычисление расстояния ===
        Console.WriteLine("Анализ структуры файла...");
        var (uniqueXCount, uniqueYCount, xMin, _, yMin, yMax, distanceXMeters, distanceYMeters)
            = GetGridInfoAndDistance(csvPath);

        Console.WriteLine($"Размер сетки: {uniqueXCount} x {uniqueYCount} = {uniqueXCount * uniqueYCount} точек");
        Console.WriteLine($"Расстояние между точками:");
        Console.WriteLine($"  По X: {distanceXMeters:F2} м");
        Console.WriteLine($"  По Y: {distanceYMeters:F2} м");
        Console.WriteLine($"  Среднее: {(distanceXMeters + distanceYMeters) / 2:F2} м");

        // === Определение шага прореживания ===
        // Если расстояние меньше 15 метров - прореживаем до ~15 метров
        // Если больше - оставляем как есть
        double targetDistance = 15.0; // Целевое расстояние в метрах
        double avgDistance = (distanceXMeters + distanceYMeters) / 2.0;

        int step = 1;
        if (avgDistance < targetDistance && avgDistance > 0)
        {
            step = Math.Max(1, (int)Math.Ceiling(targetDistance / avgDistance));
            Console.WriteLine($"Автоматическое прореживание: шаг {step} (целевое расстояние {targetDistance} м)");
        }
        else
        {
            Console.WriteLine($"Прореживание не требуется (расстояние >= {targetDistance} м)");
        }

        int meshWidth = (uniqueXCount + step - 1) / step;
        int meshHeight = (uniqueYCount + step - 1) / step;

        // === Второй проход: чтение точек ===
        Console.WriteLine("Чтение точек...");
        var x = new List<double>();
        var y = new List<double>();
        var z = new List<double>();

        ReadPointsWithStep(csvPath, step, x, y, z);

        Console.WriteLine($"Загружено вершин: {x.Count} (из {uniqueXCount * uniqueYCount})");

        // === Генерация треугольников ===
        var iTri = new List<int>();
        var jTri = new List<int>();
        var kTri = new List<int>();

        for (int row = 0; row < meshHeight - 1; row++)
        {
            for (int col = 0; col < meshWidth - 1; col++)
            {
                int topLeft = row * meshWidth + col;
                int topRight = topLeft + 1;
                int bottomLeft = (row + 1) * meshWidth + col;
                int bottomRight = bottomLeft + 1;

                iTri.Add(topLeft);
                jTri.Add(topRight);
                kTri.Add(bottomLeft);

                iTri.Add(topRight);
                jTri.Add(bottomRight);
                kTri.Add(bottomLeft);
            }
        }

        Console.WriteLine($"Треугольников: {iTri.Count}");

        // === Нормализация координат ===
        double zMin = z.Min();
        double avgLat = (yMin + yMax) / 2.0;
        double metersPerDegreeLon = 111320.0 * Math.Cos(avgLat * Math.PI / 180.0);
        double metersPerDegreeLat = 111320.0;

        var xMeters = x.Select(v => (v - xMin) * metersPerDegreeLon).ToList();
        var yMeters = y.Select(v => (v - yMin) * metersPerDegreeLat).ToList();
        var zMeters = z.Select(v => v - zMin).ToList();

        // === Создание Mesh3D ===
        var chart = Chart3D.Chart.Mesh3D<double, double, double, int, int, int, string>(
            xMeters,
            yMeters,
            zMeters,
             iTri,
            jTri,
            kTri,
            Name: "Terrain",
            ColorScale: StyleParam.Colorscale.Viridis
        );

        chart.Show();
        chart.SaveHtml(outputHtmlPath);
        Console.WriteLine($"Mesh сохранён в: {outputHtmlPath}");
    }

    public static void GenerateCubePoints(string outputPath, int size = 4, int step = 1)
    {
        using (var writer = new StreamWriter(outputPath))
        {
            writer.WriteLine("X;Y;Z");

            for (int x = 0; x <= size; x += step)
            {
                for (int y = 0; y <= size; y += step)
                {
                    for (int z = 0; z <= size; z += step)
                    {
                        writer.WriteLine($"{x};{y};{z}");
                    }
                }
            }
        }
    }
}