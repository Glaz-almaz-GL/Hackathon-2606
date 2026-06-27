using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace TerrainNavigation.Core.Navigation
{
    /// <summary>
    /// Генератор NMEA сообщений (радиовысотомер).
    /// </summary>
    public static class NmeaGenerator
    {
        private static readonly Random _rnd = new();

        /// <summary>
        /// Генерация NMEA строки GGA (упрощённая модель).
        /// </summary>
        public static string Generate(double altitudeMeters, double terrainHeight)
        {
            // Радиовысота = высота - рельеф + шум
            double noise = (_rnd.NextDouble() - 0.5) * 2.0; // ±1 м шум
            double radarAlt = altitudeMeters - terrainHeight + noise;

            if (radarAlt < 0) radarAlt = 0;

            string time = DateTime.UtcNow.ToString("HHmmss.fff", CultureInfo.InvariantCulture);

            // NMEA 0183 GGA (Global Positioning Fix Data — глобальные данные)
            return $"$GPGGA,{time},,,,,,,,{radarAlt:F1},M,,M,,*00";
        }
    }
}
