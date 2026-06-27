using System;
using System.Collections.Generic;
using System.Text;
using TerrainNavigation.Core.Models;

namespace TerrainNavigation.Core.Navigation
{
    /// <summary>
    /// Симулятор полёта самолёта (Aircraft Simulator — симулятор воздушного судна).
    /// </summary>
    public sealed class AircraftSimulator
    {
        private readonly Random _rnd = new();

        public AircraftState State { get; }

        private readonly TerrainMap _map;

        public AircraftSimulator(TerrainMap map)
        {
            _map = map;

            State = new AircraftState
            {
                X = (_map.Columns * _map.CellSizeX) / 2,
                Y = (_map.Rows * _map.CellSizeY) / 2,
                Speed = 60, // м/с
                Heading = 45,
                Altitude = 1500
            };
        }

        /// <summary>
        /// Один шаг симуляции (time step — шаг времени).
        /// </summary>
        public void Step(double dt)
        {
            // небольшой шум курса (реалистичность)
            State.Heading += (_rnd.NextDouble() - 0.5) * 2.0;

            double rad = State.Heading * Math.PI / 180.0;

            // обновление позиции
            State.X += Math.Cos(rad) * State.Speed * dt;
            State.Y += Math.Sin(rad) * State.Speed * dt;

            // лёгкие колебания скорости
            State.Speed += (_rnd.NextDouble() - 0.5) * 0.5;
        }

        /// <summary>
        /// Получить радиовысотомер (Radar Altimeter — высотомер).
        /// </summary>
        public string GetNmea()
        {
            int row = _map.GetRowFromMeters(State.Y);
            int col = _map.GetColFromMeters(State.X);

            float terrain = 0;

            if (_map.IsInside(row, col))
                terrain = _map.Heights[row, col].Height;

            return NmeaGenerator.Generate(State.Altitude, terrain);
        }
    }
}
