using System;
using System.Collections.Generic;
using System.Text;
using TerrainNavigation.Core.Models;

namespace TerrainNavigation.Core.Navigation
{
    /// <summary>
    /// Генератор профиля полёта по нарисованному пути.
    /// </summary>
    public sealed class FlightPathGenerator
    {
        private readonly TerrainMap _map;

        public FlightPathGenerator(TerrainMap map)
        {
            _map = map;
        }

        /// <summary>
        /// Преобразует путь мыши в траекторию с высотами и шумом.
        /// </summary>
        public (AircraftState[] states, double[] radar) Generate(
            FlightPath path,
            double altitude,
            double noiseMeters)
        {
            var states = new List<AircraftState>();
            var radar = new List<double>();

            foreach (var p in path.Points)
            {
                int r = _map.GetRowFromMeters(p.Y);
                int c = _map.GetColFromMeters(p.X);

                float terrain = 0;

                if (_map.IsInside(r, c))
                    terrain = _map.Heights[r, c].Height ;

                double radarAlt = altitude - terrain;

                radarAlt = NoisySensorModel.ApplyNoise(radarAlt, noiseMeters);

                states.Add(new AircraftState
                {
                    X = p.X,
                    Y = p.Y,
                    Altitude = altitude
                });

                radar.Add(radarAlt);
            }

            return (states.ToArray(), radar.ToArray());
        }
    }
}
