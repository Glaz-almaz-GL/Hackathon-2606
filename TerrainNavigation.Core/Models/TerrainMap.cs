using System;
using System.Collections.Generic;
using System.Text;

namespace TerrainNavigation.Core.Models
{
    /// <summary>
    /// Цифровая модель рельефа.
    /// Хранит высоты, производные карты и геометрические параметры.
    /// Используется как основа для алгоритмов TRN (навигации по рельефу).
    /// </summary>
    public sealed class TerrainMap
    {
        public const float NoData = float.NaN;

        /// <summary>
        /// Матрица абсолютных высот (метры над уровнем моря).
        /// Индексация: [row, column].
        /// </summary>
        public TerrainPoint[,] Heights { get; }

        /// <summary>
        /// Градиент по оси X (изменение высоты по долготе / восток-запад).
        /// </summary>
        public float[,] GradientX { get; }

        /// <summary>
        /// Градиент по оси Y (изменение высоты по широте / север-юг).
        /// </summary>
        public float[,] GradientY { get; }

        /// <summary>
        /// Уклон местности (модуль градиента).
        /// </summary>
        public float[,] Slope { get; }

        /// <summary>
        /// Шероховатость рельефа (локальная дисперсия высот).
        /// </summary>
        public float[,] Roughness { get; }

        /// <summary>
        /// Количество строк сетки.
        /// </summary>
        public int Rows => Heights.GetLength(0);

        /// <summary>
        /// Количество столбцов сетки.
        /// </summary>
        public int Columns => Heights.GetLength(1);

        /// <summary>
        /// Минимальная долгота карты.
        /// </summary>
        public double MinLongitude { get; }

        /// <summary>
        /// Максимальная долгота карты.
        /// </summary>
        public double MaxLongitude { get; }

        /// <summary>
        /// Минимальная широта карты.
        /// </summary>
        public double MinLatitude { get; }

        /// <summary>
        /// Максимальная широта карты.
        /// </summary>
        public double MaxLatitude { get; }

        /// <summary>
        /// Шаг сетки по долготе (градусы на пиксель сетки).
        /// </summary>
        public double LongitudeStep { get; }

        /// <summary>
        /// Шаг сетки по широте (градусы на пиксель сетки).
        /// </summary>
        public double LatitudeStep { get; }

        /// <summary>
        /// Размер клетки в метрах по X (приближенно, локальная проекция).
        /// </summary>
        public double CellSizeX { get; }

        /// <summary>
        /// Размер клетки в метрах по Y (приближенно, локальная проекция).
        /// </summary>
        public double CellSizeY { get; }

        /// <summary>
        /// Минимальная высота на карте.
        /// </summary>
        public float MinHeight { get; set; }

        /// <summary>
        /// Максимальная высота на карте.
        /// </summary>
        public float MaxHeight { get; set; }

        /// <summary>
        /// Средняя высота.
        /// </summary>
        public float AverageHeight { get; set; }

        /// <summary>
        /// Стандартное отклонение высот.
        /// </summary>
        public float HeightStdDev { get; set; }

        /// <summary>
        /// Создание цифровой модели рельефа.
        /// </summary>
        public TerrainMap(
            TerrainPoint[,] heights,

            double minLongitude,
            double maxLongitude,
            double minLatitude,
            double maxLatitude,
            double longitudeStep,
            double latitudeStep,
            double cellSizeX,
            double cellSizeY)
        {
            Heights = heights;


            MinLongitude = minLongitude;
            MaxLongitude = maxLongitude;
            MinLatitude = minLatitude;
            MaxLatitude = maxLatitude;

            LongitudeStep = longitudeStep;
            LatitudeStep = latitudeStep;

            CellSizeX = cellSizeX;
            CellSizeY = cellSizeY;
        }

        /// <summary>
        /// Возвращает высоту в узле сетки.
        /// </summary>
        public float GetHeight(int row, int column)
        {
            return Heights[row, column].Height;
        }

        /// <summary>
        /// Перевод широты в индекс строки массива.
        /// </summary>
        public int GetRow(double latitude)
        {
            return (int)Math.Round((latitude - MinLatitude) / LatitudeStep);
        }

        /// <summary>
        /// Перевод долготы в индекс столбца массива.
        /// </summary>
        public int GetColumn(double longitude)
        {
            return (int)Math.Round((longitude - MinLongitude) / LongitudeStep);
        }

        /// <summary>
        /// Перевод строки массива в широту.
        /// </summary>
        public double GetLatitude(int row)
        {
            return MinLatitude + row * LatitudeStep;
        }

        /// <summary>
        /// Перевод столбца массива в долготу.
        /// </summary>
        public double GetLongitude(int column)
        {
            return MinLongitude + column * LongitudeStep;
        }

        /// <summary>
        /// Проверка выхода за границы карты.
        /// </summary>
        public bool IsInside(int row, int column)
        {
            return row >= 0 && column >= 0 &&
                   row < Rows && column < Columns;
        }

        /// <summary>
        /// Перевод шага сетки в метры (приближенно).
        /// Используется для TRN алгоритмов.
        /// </summary>
        public double GetDistanceMeters(double latitudeDeltaDegrees)
        {
            // 1 градус ≈ 111320 метров (приближение)
            return latitudeDeltaDegrees * 111320.0;
        }

        public int GetRowFromMeters(double yMeters)
        {
            return (int)(yMeters / CellSizeY);
        }

        public int GetColFromMeters(double xMeters)
        {
            return (int)(xMeters / CellSizeX);
        }
    }
}
