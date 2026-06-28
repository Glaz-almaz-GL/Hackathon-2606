using OSGeo.GDAL;
using TerrainScanner.Core.Models.Terrain;
using TerrainScanner.Core.Services;

namespace TerrainScanner.Core
{
    public class MapLoader
    {
        /// <summary>
        /// Загружает карту из GeoTIFF файла.
        /// </summary>
        public TerrainMap LoadFromTif(string filePath, CancellationToken cancellationToken = default)
        {
            LogService.Log($"Начало загрузки карты из файла: {filePath}");

            if (!File.Exists(filePath))
            {
                LogService.Log($"ОШИБКА: Файл не найден: {filePath}");
                throw new FileNotFoundException("Файл TIFF не найден", filePath);
            }

            using Dataset ds = Gdal.Open(filePath, Access.GA_ReadOnly);
            if (ds == null)
            {
                LogService.Log($"ОШИБКА: GDAL не смог открыть файл: {filePath}");
                throw new InvalidDataException($"Не удалось открыть GeoTIFF файл: {filePath}");
            }

            int width = ds.RasterXSize;  // Количество столбцов (Columns)
            int height = ds.RasterYSize; // Количество строк (Rows)

            if (width <= 0 || height <= 0)
            {
                LogService.Log($"ОШИБКА: Некорректные размеры растра: {width}x{height}");
                throw new InvalidDataException("Некорректные размеры растра");
            }

            LogService.Log($"Размер растра: {width} x {height} пикселей");

            // Считываем аффинное преобразование (GeoTransform)
            double[] gt = new double[6];
            ds.GetGeoTransform(gt);

            // 1. Вычисляем границы карты из GeoTransform (учитываем все 4 угла на случай ротации)
            double lon1 = gt[0];
            double lat1 = gt[3];
            double lon2 = gt[0] + (width * gt[1]);
            double lat2 = gt[3] + (width * gt[4]);
            double lon3 = gt[0] + (height * gt[2]);
            double lat3 = gt[3] + (height * gt[5]);
            double lon4 = gt[0] + (width * gt[1]) + (height * gt[2]);
            double lat4 = gt[3] + (width * gt[4]) + (height * gt[5]);

            double minLon = Math.Min(Math.Min(lon1, lon2), Math.Min(lon3, lon4));
            double maxLon = Math.Max(Math.Max(lon1, lon2), Math.Max(lon3, lon4));
            double minLat = Math.Min(Math.Min(lat1, lat2), Math.Min(lat3, lat4));
            double maxLat = Math.Max(Math.Max(lat1, lat2), Math.Max(lat3, lat4));

            LogService.Log($"Географические границы: Lon [{minLon:F6}, {maxLon:F6}], Lat [{minLat:F6}, {maxLat:F6}]");

            // 2. Вычисляем шаги сетки в градусах
            double lonStep = Math.Abs(gt[1]);
            double latStep = Math.Abs(gt[5]);

            using Band band = ds.GetRasterBand(1);
            if (band == null)
            {
                LogService.Log($"ОШИБКА: Не удалось получить растровый слой (Band 1) из файла: {filePath}");
                throw new InvalidDataException("Не удалось получить растровый слой");
            }

            band.GetNoDataValue(out double noDataValue, out int hasNoData);

            if (hasNoData == 1)
            {
                LogService.Log($"Значение NoData: {noDataValue}");
            }
            else
            {
                LogService.Log("Значение NoData не определено в файле");
            }

            // 3. Читаем данные напрямую в плоский массив
            float[] zValues = new float[width * height];
            LogService.Log($"Чтение данных растра размером {width}x{height} в память...");

            band.ReadRaster(0, 0, width, height, zValues, width, height, 0, 0);

            cancellationToken.ThrowIfCancellationRequested();

            LogService.Log($"Данные прочитаны. Всего значений: {zValues.Length:N0}");

            // 4. Обработка NoData и поиск Min/Max высот за один проход
            float minH = float.MaxValue;
            float maxH = float.MinValue;
            bool hasValidData = false;
            int noDataCount = 0;

            for (int i = 0; i < zValues.Length; i++)
            {
                // Заменяем NoData на NaN для сохранения структуры сетки
                if (hasNoData == 1 && Math.Abs(zValues[i] - noDataValue) < 0.001)
                {
                    zValues[i] = float.NaN;
                    noDataCount++;
                }
                else
                {
                    if (zValues[i] < minH)
                    {
                        minH = zValues[i];
                    }

                    if (zValues[i] > maxH)
                    {
                        maxH = zValues[i];
                    }

                    hasValidData = true;
                }
            }

            if (!hasValidData)
            {
                LogService.Log("ОШИБКА: В файле TIFF нет валидных данных высот");
                throw new InvalidDataException("В файле TIFF нет валидных данных высот");
            }

            LogService.Log($"Обработка данных завершена. NoData ячеек: {noDataCount:N0}, Валидных: {zValues.Length - noDataCount:N0}");
            LogService.Log($"Диапазон высот: {minH:F2} - {maxH:F2} м");

            // 5. Вычисляем размер клетки в метрах (аппроксимация для центра карты)
            double centerLatRad = (minLat + maxLat) / 2.0 * Math.PI / 180.0;
            double cellSizeY = latStep * 111320.0;
            double cellSizeX = lonStep * 111320.0 * Math.Cos(centerLatRad);

            LogService.Log($"Размер клетки: {cellSizeX:F2} x {cellSizeY:F2} м (аппроксимация для центра карты)");

            LogService.Log("Инициализация объекта TerrainMap...");

            MapOptions options = new(
                Heights: zValues,
                Rows: height,
                Columns: width,
                MinLongitude: minLon,
                MaxLongitude: maxLon,
                MinLatitude: minLat,
                MaxLatitude: maxLat,
                LongitudeStep: lonStep,
                LatitudeStep: latStep,
                MinHeight: minH,
                MaxHeight: maxH,
                CellSizeX: cellSizeX,
                CellSizeY: cellSizeY
            );

            TerrainMap map = new(options);

            LogService.Log(
                $"Карта успешно загружена:\n" +
                $"  Размеры: {map.Rows} x {map.Columns}\n" +
                $"  Высоты: {map.MinHeight:F2} - {map.MaxHeight:F2} м\n" +
                $"  Размер клетки: {map.CellSizeX:F2} x {map.CellSizeY:F2} м\n" +
                $"  Границы: Lon [{map.MinLongitude:F6}, {map.MaxLongitude:F6}], Lat [{map.MinLatitude:F6}, {map.MaxLatitude:F6}]");

            return map;
        }
    }
}