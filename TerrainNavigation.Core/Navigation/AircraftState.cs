using System;
using System.Collections.Generic;
using System.Text;

namespace TerrainNavigation.Core.Navigation
{
    /// <summary>
    /// Состояние самолёта (Aircraft State — состояние воздушного судна).
    /// </summary>
    public sealed class AircraftState
    {
        /// <summary>
        /// Позиция X (метры в локальной системе координат).
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Позиция Y (метры в локальной системе координат).
        /// </summary>
        public double Y { get; set; }

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
