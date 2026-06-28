using System.Globalization;
using TerrainScanner.Core.Services;

namespace TerrainScanner.Core.Generators
{
    /// <summary>
    /// Генератор NMEA сообщений (радиовысотомер).
    /// </summary>
    public sealed class NmeaGenerator
    {
        private readonly Random _rnd = new();

        /// <summary>
        /// Генерация NMEA строки GGA (упрощённая модель).
        /// </summary>
        /// <param name="altitudeMeters">Высота дрона над уровнем моря (метры).</param>
        /// <param name="terrainHeight">Высота рельефа под дроном (метры над уровнем моря).</param>
        /// <param name="noiseMin">Минимальное значение шума (метры). Если null — используется диапазон [0, 30].</param>
        /// <param name="noiseMax">Максимальное значение шума (метры). Если null — используется диапазон [0, 30].</param>
        public string Generate(double altitudeMeters, double terrainHeight, double? noiseMin = null, double? noiseMax = null)
        {
            double noise;

            // Радиовысота = высота - рельеф + шум
            if (noiseMin != null && noiseMax != null)
            {
                noise = noiseMin.Value + (_rnd.NextDouble() * (noiseMax.Value - noiseMin.Value));
            }
            else
            {
                noise = 0 + (_rnd.NextDouble() * (30 - 0));
            }

            double radarAlt = altitudeMeters - terrainHeight + noise;

            if (radarAlt < 0)
            {
                radarAlt = 0;
            }

            string time = DateTime.UtcNow.ToString("HHmmss.fff", CultureInfo.InvariantCulture);

            // NMEA 0183 GGA (Global Positioning Fix Data — глобальные данные)
            string result = $"$GPGGA,{time},,,,,,,,{radarAlt:F1},M,,M,,*00";

            LogService.Log($"Generated Nmea Data: {result}");

            return result;
        }

        /// <summary>
        /// Парсит значение радиовысоты из NMEA-строки GGA.
        /// </summary>
        /// <param name="nmeaSentence">NMEA-строка формата GGA.</param>
        /// <returns>Значение высоты или null, если данные отсутствуют или некорректны.</returns>
        public static float? ParseRadarAltitude(string nmeaSentence)
        {
            if (string.IsNullOrWhiteSpace(nmeaSentence))
                return null;

            var parts = nmeaSentence.Split(',');

            // Проверяем, что строка содержит достаточно полей и поле не пустое
            if (parts.Length > 9 && !string.IsNullOrEmpty(parts[9]) && float.TryParse(parts[9], NumberStyles.Float, CultureInfo.InvariantCulture, out float altitude))
            {
                return altitude;
            }

            return null; // Возвращаем null, если данные отсутствуют (NoData) или некорректны
        }
    }
}