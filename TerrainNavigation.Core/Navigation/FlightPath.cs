using System;
using System.Collections.Generic;
using System.Text;

namespace TerrainNavigation.Core.Navigation
{
    /// <summary>
    /// Реальный маршрут самолёта (Ground Truth Path — истинный путь).
    /// </summary>
    public sealed class FlightPath
    {
        public List<(double X, double Y)> Points { get; } = new();
    }
}
