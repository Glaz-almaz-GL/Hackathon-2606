using OSGeo.GDAL;
using System.Globalization;
using TerrainNavigation.Core.Models;

namespace TerrainNavigation.Core.Map
{
    /// <summary>
    /// Загрузчик цифровой модели рельефа.
    /// Формирует TerrainMap (карта рельефа) и вычисляет все производные.
    /// </summary>
    public sealed class MapLoader
    {
        /// <summary>
        /// Загружает карту из CSV файла формата: Lat;Lon;Height
        /// </summary>
        public TerrainMap LoadFromCsv(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Файл карты не найден", filePath);
            }

            string[] lines = File.ReadAllLines(filePath);

            if (lines.Length < 2)
            {
                throw new InvalidDataException("Файл карты пуст или повреждён");
            }

            int startIndex = 0;

            // Проверка заголовка
            if (lines[0].Contains("Lat") && lines[0].Contains("Lon") && lines[0].Contains("Height"))
            {
                startIndex = 1;
            }

            List<TerrainPoint> terrainPoints = new(lines.Length - startIndex);

            // -----------------------------
            // 1. Чтение данных
            // -----------------------------
            for (int i = startIndex; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue;

                string[] parts = lines[i].Split(';', ',');

                if (parts.Length < 3) continue;

                try
                {
                    double lat = double.Parse(parts[0], CultureInfo.InvariantCulture);
                    double lon = double.Parse(parts[1], CultureInfo.InvariantCulture);
                    float height = float.Parse(parts[2], CultureInfo.InvariantCulture);

                    terrainPoints.Add(new TerrainPoint
                    {
                        Latitude = lat,
                        Longitude = lon,
                        Height = height
                    });
                }
                catch (FormatException)
                {
                    continue;
                }
            }

            if (terrainPoints.Count < 9)
            {
                throw new InvalidDataException("Недостаточно точек для построения карты");
            }

            // -----------------------------
            // 2. Определение размеров сетки
            // -----------------------------
            int pointInLineCount = 1;
            double firstLat = terrainPoints[0].Latitude;

            for (int i = 1; i < terrainPoints.Count; i++)
            {
                if (terrainPoints[i].Latitude != firstLat)
                {
                    pointInLineCount = i;
                    break;
                }
            }

            int xSize = pointInLineCount;
            int ySize = terrainPoints.Count / pointInLineCount;

            // -----------------------------
            // 3. Заполнение массива сетки
            // -----------------------------
            TerrainPoint[,] heights = new TerrainPoint[xSize, ySize];

            for (int row = 0; row < ySize; row++)
            {
                for (int col = 0; col < xSize; col++)
                {
                    int index = (row * xSize) + col;
                    if (index < terrainPoints.Count)
                    {
                        heights[col, row] = terrainPoints[index];
                    }
                }
            }

            // -----------------------------
            // 4. Делегирование общей логики
            // -----------------------------
            return CreateTerrainMapFromGrid(heights, xSize, ySize);
        }

        /// <summary>
        /// Загружает карту из GeoTIFF файла.
        /// </summary>
        public TerrainMap LoadFromTif(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Файл TIFF не найден", filePath);
            }

            using Dataset ds = Gdal.Open(filePath, Access.GA_ReadOnly);
            ArgumentNullException.ThrowIfNull(ds);

            int width = ds.RasterXSize;
            int height = ds.RasterYSize;

            if (width <= 0 || height <= 0)
            {
                throw new InvalidDataException("Некорректные размеры растра");
            }

            double[] gt = new double[6];
            ds.GetGeoTransform(gt);

            using Band band = ds.GetRasterBand(1);
            ArgumentNullException.ThrowIfNull(band);

            band.GetNoDataValue(out double noDataValue, out int hasNoData);

            float[] zValues = new float[width * height];
            band.ReadRaster(0, 0, width, height, zValues, width, height, 0, 0);

            TerrainPoint[,] heights = new TerrainPoint[width, height];
            bool hasValidData = false;

            // Порядок обхода: строки (Y), затем столбцы (X)
            for (int row = 0; row < height; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    int bufferIndex = (row * width) + col;
                    float z = zValues[bufferIndex];

                    // Пропускаем NoData
                    if (hasNoData == 1 && Math.Abs(z - noDataValue) < 0.001)
                    {
                        heights[col, row] = new TerrainPoint { Height = 0, Latitude = 0, Longitude = 0 };
                        continue;
                    }

                    hasValidData = true;

                    // Вычисляем координаты центра пикселя
                    double lon = gt[0] + ((col + 0.5) * gt[1]) + ((row + 0.5) * gt[2]);
                    double lat = gt[3] + ((col + 0.5) * gt[4]) + ((row + 0.5) * gt[5]);

                    heights[col, row] = new TerrainPoint
                    {
                        Longitude = lon,
                        Latitude = lat,
                        Height = z
                    };
                }
            }

            if (!hasValidData)
            {
                throw new InvalidDataException("В файле TIFF нет валидных данных высот");
            }

            // -----------------------------
            // Делегирование общей логики
            // -----------------------------
            return CreateTerrainMapFromGrid(heights, width, height);
        }

        /// <summary>
        /// Общий метод для создания TerrainMap из готовой сетки точек.
        /// Вычисляет границы, статистику и шаги сетки.
        /// </summary>
        private TerrainMap CreateTerrainMapFromGrid(TerrainPoint[,] heights, int xSize, int ySize)
        {
            double minX = double.MaxValue;
            double maxX = double.MinValue;
            double minY = double.MaxValue;
            double maxY = double.MinValue;

            float minH = float.MaxValue;
            float maxH = float.MinValue;
            double sum = 0;
            double sumSq = 0;
            int validCount = 0;

            // Проход по сетке для сбора статистики
            for (int row = 0; row < ySize; row++)
            {
                for (int col = 0; col < xSize; col++)
                {
                    var p = heights[col, row];

                    // Пропускаем пустые/невалидные точки (помеченные нулями в TIF или отсутствующие в CSV)
                    // Примечание: Если высота 0 является валидной, нужна более сложная проверка (например, флаг IsEmpty)
                    if (p.Latitude == 0 && p.Longitude == 0 && p.Height == 0)
                    {
                        continue;
                    }

                    if (p.Latitude < minX) minX = p.Latitude;
                    if (p.Latitude > maxX) maxX = p.Latitude;
                    if (p.Longitude < minY) minY = p.Longitude;
                    if (p.Longitude > maxY) maxY = p.Longitude;

                    if (p.Height < minH) minH = p.Height;
                    if (p.Height > maxH) maxH = p.Height;

                    sum += p.Height;
                    sumSq += p.Height * p.Height;
                    validCount++;
                }
            }

            if (validCount == 0)
            {
                throw new InvalidDataException("Нет валидных данных для построения карты");
            }

            float avg = (float)(sum / validCount);
            float std = (float)Math.Sqrt((sumSq / validCount) - (avg * avg));

            // Вычисляем шаг сетки
            double xStep = 0;
            double yStep = 0;

            if (xSize > 1)
            {
                // Ищем первую валидную пару по X
                for (int c = 1; c < xSize; c++)
                {
                    if (heights[c, 0].Longitude != 0)
                    {
                        xStep = Math.Abs(heights[c, 0].Longitude - heights[0, 0].Longitude);
                        break;
                    }
                }
            }

            if (ySize > 1)
            {
                // Ищем первую валидную пару по Y
                for (int r = 1; r < ySize; r++)
                {
                    if (heights[0, r].Latitude != 0)
                    {
                        yStep = Math.Abs(heights[0, r].Latitude - heights[0, 0].Latitude);
                        break;
                    }
                }
            }

            return new TerrainMap(
                heights,
                minX,
                maxX,
                minY,
                maxY,
                xStep,
                yStep,
                xStep * 111320.0,
                yStep * 111320.0)
            {
                MinHeight = minH,
                MaxHeight = maxH,
                AverageHeight = avg,
                HeightStdDev = std
            };
        }
    }
}