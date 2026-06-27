using System.Text;

namespace TerrainScanner.Core.Models
{
    public sealed class TerrainMap
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
            return _heights[(row * Columns) + column];
        }

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
            return (int)Math.Round((latitude - MinLatitude) / LatitudeStep);
        }

        public int GetColumn(double longitude)
        {
            return (int)Math.Round((longitude - MinLongitude) / LongitudeStep);
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
                    writer.Write(GetHeight(r, c));
                    if (c < Columns - 1)
                    {
                        writer.Write(';');
                    }
                }
                writer.WriteLine();
            }
        }

        #endregion
    }
}