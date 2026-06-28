using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace TerrainScanner.Core.Models.Terrain
{
    public sealed class TerrainMap : IDisposable
    {
        // Плоский массив для максимальной производительности и локальности кэша
        private readonly float[] _heights;

        public int Rows { get; }
        public int Columns { get; }

        public double MinLongitude { get; }
        public double MaxLongitude { get; }
        public double MinLatitude { get; }
        public double MaxLatitude { get; }

        public double LongitudeStep { get; }
        public double LatitudeStep { get; }

        /// <summary>
        /// Размер клетки по Y в метрах (константа).
        /// </summary>
        public double CellSizeY { get; }

        /// <summary>
        /// Размер клетки по X в метрах (аппроксимация для центра карты).
        /// </summary>
        public double CellSizeX { get; }

        public float MinHeight { get; }
        public float MaxHeight { get; }

        private bool _disposed;

        public TerrainMap(MapOptions options)
        {
            if (options.Heights.Length != options.Rows * options.Columns)
            {
                throw new InvalidDataException("Размер массива высот не соответствует размерам сетки.");
            }

            _heights = options.Heights;
            Rows = options.Rows;
            Columns = options.Columns;

            MinLongitude = options.MinLongitude;
            MaxLongitude = options.MaxLongitude;
            MinLatitude = options.MinLatitude;
            MaxLatitude = options.MaxLatitude;
            LongitudeStep = options.LongitudeStep;
            LatitudeStep = options.LatitudeStep;

            MinHeight = options.MinHeight;
            MaxHeight = options.MaxHeight;

            // Аппроксимация размера клетки в метрах (вычисляется для центра карты)
            CellSizeY = options.CellSizeY;
            CellSizeX = options.CellSizeX;
        }

        #region Indexers & Getters

        public float GetHeight(int row, int column)
        {
            if (!IsInside(row, column))
                throw new ArgumentOutOfRangeException($"Координаты [{row}, {column}] выходят за пределы карты [{Rows}x{Columns}].");

            return _heights![(row * Columns) + column];
        }
        public ReadOnlyMemory<float> GetHeightMap() => new(_heights);

        public TerrainPoint GetPoint(int row, int column)
        {
            return new TerrainPoint(
                GetLongitude(column),
                GetLatitude(row),
                GetHeight(row, column)
            );
        }

        public bool HasData(int row, int column)
        {
            return !float.IsNaN(GetHeight(row, column));
        }

        #endregion

        #region Coordinate Conversions

        public int GetRow(double latitude)
        {
            return (int)Math.Floor((latitude - MinLatitude) / LatitudeStep);
        }

        public int GetColumn(double longitude)
        {
            return (int)Math.Floor((longitude - MinLongitude) / LongitudeStep);
        }

        public double GetLatitude(int row)
        {
            return MinLatitude + (row * LatitudeStep);
        }

        public double GetLongitude(int column)
        {
            return MinLongitude + (column * LongitudeStep);
        }

        public bool IsInside(int row, int column)
        {
            return row >= 0 && column >= 0 && row < Rows && column < Columns;
        }

        public int GetRowFromMeters(double yMeters)
        {
            return (int)(yMeters / CellSizeY);
        }

        public int GetColFromMeters(double xMeters)
        {
            return (int)(xMeters / CellSizeX);
        }

        /// <summary>
        /// Переводит разницу широт в метры.
        /// </summary>
        public static double LatitudeDeltaToMeters(double latitudeDeltaDegrees)
        {
            return latitudeDeltaDegrees * 111320.0;
        }

        /// <summary>
        /// Переводит разницу долгот в метры для заданной широты.
        /// </summary>
        public static double LongitudeDeltaToMeters(double longitudeDeltaDegrees, double atLatitude)
        {
            return longitudeDeltaDegrees * 111320.0 * Math.Cos(atLatitude * Math.PI / 180.0);
        }

        #endregion

        #region Serialization

        public void SaveAsCsv(string csvPath, CancellationToken cancellationToken = default)
        {
            using StreamWriter writer = new(csvPath, false, Encoding.UTF8);

            // Запись метаданных (заголовок)
            writer.WriteLine("MinLongitude;MaxLongitude;MinLatitude;MaxLatitude;Rows;Columns");
            writer.WriteLine($"{MinLongitude};{MaxLongitude};{MinLatitude};{MaxLatitude};{Rows};{Columns}");

            for (int r = 0; r < Rows; r++)
            {
                // Проверка отмены операции для долгих процессов
                cancellationToken.ThrowIfCancellationRequested();

                for (int c = 0; c < Columns; c++)
                {
                    writer.Write(GetHeight(r, c).ToString(CultureInfo.InvariantCulture));
                    if (c < Columns - 1)
                    {
                        writer.Write(';');
                    }
                }
                writer.WriteLine();
            }
        }

        public System.Drawing.Bitmap ToBitmap()
        {
            // Создаем битмап с форматом 24 бита на пиксель (BGR)
            Bitmap bitmap = new(Columns, Rows, PixelFormat.Format24bppRgb);
            Rectangle rect = new(0, 0, Columns, Rows);

            // Блокируем биты для прямой записи в память
            var bmpData = bitmap.LockBits(rect, ImageLockMode.WriteOnly, bitmap.PixelFormat);

            int bytesPerPixel = 3; // 24 бита = 3 байта
            int stride = bmpData.Stride; // Ширина строки в байтах (с учетом выравнивания)
            byte[] rowBuffer = new byte[stride];

            // Вычисляем диапазон высот для нормализации
            float range = MaxHeight - MinHeight;
            if (range <= float.Epsilon)
            {
                range = 1f; // Защита от деления на ноль, если все высоты одинаковы
            }

            IntPtr currentScanline = bmpData.Scan0;

            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Columns; c++)
                {
                    float h = GetHeight(r, c);
                    byte gray;

                    // Обработка пропусков данных (NoData), которые были заменены на NaN при загрузке
                    if (float.IsNaN(h))
                    {
                        gray = 0; // Черный цвет для "дыр" в рельефе
                    }
                    else
                    {
                        // Нормализация высоты к диапазону [0, 255] относительно Min/Max высот карты.
                        // Это дает максимальный контраст для визуализации конкретного участка.
                        float normalized = (h - MinHeight) / range;
                        gray = (byte)Math.Clamp(normalized * 255f, 0, 255);
                    }

                    int offset = c * bytesPerPixel;

                    // Формат пикселя в памяти: BGR (Синий, Зеленый, Красный)
                    rowBuffer[offset] = gray;     // B
                    rowBuffer[offset + 1] = gray; // G
                    rowBuffer[offset + 2] = gray; // R
                }

                // Копируем готовую строку пикселей в память битмапа
                Marshal.Copy(rowBuffer, 0, currentScanline, stride);

                // Переходим к следующей строке в памяти
                currentScanline = IntPtr.Add(currentScanline, stride);
            }

            bitmap.UnlockBits(bmpData);
            return bitmap;
        }

        #endregion
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                // Контролируемое освобождение
            }

            Array.Clear(_heights);
            _disposed = true;
        }
    }
}