using OSGeo.GDAL;
using System.Runtime.Intrinsics.X86;
using TerrainScanner.Core.Models;

namespace TerrainScanner.Core
{
    public class MapLoader
    {
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

            return new TerrainMap(new MapOptions(
                heights,
                MinLongitude: minX,
                MaxLongitude: maxX,
                MinLatitude: minY,
                MaxLatitude: maxY,
                LongitudeStep: xStep,
                LatitudeStep: yStep,
                MinHeight: minH,
                MaxHeight: maxH,
                CellSizeX: xStep * 111320.0,
                CellSizeY: yStep * 111320.0));
        }
    }
}