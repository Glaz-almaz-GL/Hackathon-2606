using Microsoft.Extensions.Logging;
using System.Globalization;

namespace TerrainScanner.Core.Generators
{
    /// <summary>
    /// Генератор NMEA сообщений (радиовысотомер).
    /// </summary>
    public sealed class NmeaGenerator(ILogger<NmeaGenerator>? logger = null)
    {
        private readonly ILogger<NmeaGenerator>? _logger = logger;
        private readonly Random _rnd = new();

        /// <summary>
        /// Генерация NMEA строки GGA (упрощённая модель).
        /// </summary>
        public string Generate(double altitudeMeters, double terrainHeight)
        {
            // Радиовысота = высота - рельеф + шум
            double noise = (_rnd.NextDouble() - 0.5) * 2.0; // +-1 м шум
            double radarAlt = altitudeMeters - terrainHeight + noise;

            if (radarAlt < 0)
            {
                radarAlt = 0;
            }

            string time = DateTime.UtcNow.ToString("HHmmss.fff", CultureInfo.InvariantCulture);

            // NMEA 0183 GGA (Global Positioning Fix Data — глобальные данные)
            string result = $"$GPGGA,{time},,,,,,,,{radarAlt:F1},M,,M,,*00";

            if (_logger?.IsEnabled(LogLevel.Information) == true)
            {
                _logger?.LogInformation("Generated Nmea Data: {Data}", result);
            }

            return result;
        }
    }
}
