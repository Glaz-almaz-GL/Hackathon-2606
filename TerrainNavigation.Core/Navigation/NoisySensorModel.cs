using System;
using System.Collections.Generic;
using System.Text;

namespace TerrainNavigation.Core.Navigation
{
    /// <summary>
    /// Модель шумного датчика (Sensor Noise Model — модель ошибок измерений).
    /// </summary>
    public static class NoisySensorModel
    {
        private static readonly Random _rnd = new();

        /// <summary>
        /// Добавление шума к радиовысоте.
        /// </summary>
        public static double ApplyNoise(double value, double noiseMeters)
        {
            double noise = (_rnd.NextDouble() * 2 - 1) * noiseMeters;
            return Math.Max(0, value + noise);
        }
    }
}
