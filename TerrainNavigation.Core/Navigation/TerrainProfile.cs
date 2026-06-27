using System;
using System.Collections.Generic;
using System.Text;

namespace TerrainNavigation.Core.Navigation
{
    /// <summary>
    /// Профиль рельефа (Terrain Profile — последовательность высот вдоль траектории).
    /// </summary>
    public sealed class TerrainProfile
    {
        /// <summary>
        /// Последовательность высот рельефа.
        /// </summary>
        public float[] Heights { get; }

        /// <summary>
        /// Временные метки (в секундах).
        /// </summary>
        public double[] Time { get; }

        public TerrainProfile(float[] heights, double[] time)
        {
            Heights = heights;
            Time = time;
        }
    }
}
