using System;
using System.Collections.Generic;
using System.Text;

namespace TerrainScanner.Core.Models.Models
{
    /// <summary>
    /// Точка траектории полета дрона с полной информацией
    /// </summary>
    public sealed class DroneTrackPoint
    {
        /// <summary>
        /// Широта
        /// </summary>
        public double Latitude { get; init; }

        /// <summary>
        /// Долгота
        /// </summary>
        public double Longitude { get; init; }

        /// <summary>
        /// Высота рельефа под дроном (метры над уровнем моря)
        /// </summary>
        public float TerrainHeight { get; init; }

        /// <summary>
        /// Пройденное расстояние от начала пути (метры)
        /// </summary>
        public float Distance { get; init; }
    }
}
