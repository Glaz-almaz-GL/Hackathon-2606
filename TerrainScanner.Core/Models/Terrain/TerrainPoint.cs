using System;
using System.Collections.Generic;
using System.Text;

namespace TerrainScanner.Core.Models.Terrain
{
    /// <summary>
    /// Точка цифровой модели рельефа.
    /// </summary>
    public readonly record struct TerrainPoint(double Longitude, double Latitude, float Height);
}
