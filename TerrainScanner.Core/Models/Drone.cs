using System;
using System.Collections.Generic;
using System.Text;

namespace TerrainScanner.Core.Models
{
    /// <summary>
    /// Состояние дрона
    /// </summary>
    public sealed class Drone
    {
        /// <summary>
        /// Местоположение дрона на карте, по умолчанию высота 1500, Lat и Lon равны 0.0
        /// </summary>
        public TerrainPoint Location { get; set; } = new()
        {
            Height = 1500,
            Latitude = 0.0,
            Longitude = 0.0
        };

        /// <summary>
        /// Высота над уровнем моря (барометрическая).
        /// </summary>
        public double Altitude { get; set; } = 1500;

        /// <summary>
        /// Курс (Heading — направление движения в градусах).
        /// </summary>
        public double Heading { get; set; }

        /// <summary>
        /// Скорость (Velocity — м/с).
        /// </summary>
        public double Speed { get; set; }
    }
}
