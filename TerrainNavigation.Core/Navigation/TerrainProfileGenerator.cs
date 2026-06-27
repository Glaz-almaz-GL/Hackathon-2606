using System;
using System.Collections.Generic;
using System.Text;
using TerrainNavigation.Core.Models;

namespace TerrainNavigation.Core.Navigation
{
    /// <summary>
    /// Генератор профиля рельефа вдоль траектории самолёта.
    /// </summary>
    public sealed class TerrainProfileGenerator
    {
        private readonly TerrainMap _map;

        public TerrainProfileGenerator(TerrainMap map)
        {
            _map = map;
        }

        /// <summary>
        /// Построение профиля рельефа по движению самолёта.
        /// </summary>
        public TerrainProfile Generate(AircraftState[] trajectory, double dt)
        {
            var heights = new List<float>();
            var time = new List<double>();

            for (int i = 0; i < trajectory.Length; i++)
            {
                var state = trajectory[i];

                int row = _map.GetRowFromMeters(state.Y);
                int col = _map.GetColFromMeters(state.X);

                float h = 0;

                if (_map.IsInside(row, col))
                    h = _map.Heights[row, col].Height;

                heights.Add(h);
                time.Add(i * dt);
            }

            return new TerrainProfile(heights.ToArray(), time.ToArray());
        }
    }
}
