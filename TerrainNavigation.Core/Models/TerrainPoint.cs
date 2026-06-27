using System;
using System.Collections.Generic;
using System.Text;

namespace TerrainNavigation.Core.Models
{
    /// <summary>
    /// Точка цифровой модели рельефа.
    /// </summary>
    public sealed class TerrainPoint
    {
        /// <summary>
        /// Долгота.
        /// </summary>
        public double Longitude { get; set; }

        /// <summary>
        /// Широта.
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        /// Абсолютная высота над уровнем моря.
        /// </summary>
        public float Height { get; set; }
    }
}
