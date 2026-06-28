using TerrainScanner.Core.Models;
using TerrainScanner.Core.Models.Models;
using TerrainScanner.Core.Models.Terrain;
using TerrainScanner.Core.Services;

namespace TerrainScanner.Core.Simulators
{
    public sealed class DroneFlightSimulator(
        Drone drone,
        TerrainMap map,
        double simulationDurationSeconds = 60.0,
        double updateIntervalSeconds = 0.1)
    {
        private readonly Drone _drone = drone;
        private readonly TerrainMap _map = map;
        private readonly double _simulationDurationSeconds = simulationDurationSeconds;
        private readonly double _updateIntervalSeconds = updateIntervalSeconds;

        /// <summary>
        /// Симулирует полет дрона и возвращает трек с географическими координатами
        /// </summary>
        public List<DroneTrackPoint> SimulateFlight()
        {
            LogService.Log(
                $"=== Начало симуляции полёта дрона ===\n" +
                $"  Длительность: {_simulationDurationSeconds:F2} с\n" +
                $"  Интервал обновления: {_updateIntervalSeconds:F4} с\n" +
                $"  Скорость: {_drone.Speed:F2} м/с\n" +
                $"  Курс: {_drone.Heading:F2}°\n" +
                $"  Высота: {_drone.Altitude:F2} м\n" +
                $"  Старт: ({_drone.Location.Latitude:F6}, {_drone.Location.Longitude:F6})");

            List<DroneTrackPoint> track = [];

            double currentTime = 0;
            double totalDistance = 0;

            double currentLat = _drone.Location.Latitude;
            double currentLon = _drone.Location.Longitude;

            int totalSteps = 0;
            int outOfBoundsCount = 0;
            int noDataCount = 0;

            while (currentTime <= _simulationDurationSeconds)
            {
                int row = _map.GetRow(currentLat);
                int col = _map.GetColumn(currentLon);

                if (_map.IsInside(row, col))
                {
                    float terrainHeight = _map.GetHeight(row, col);

                    if (!float.IsNaN(terrainHeight))
                    {
                        track.Add(new DroneTrackPoint
                        {
                            Latitude = currentLat,
                            Longitude = currentLon,
                            TerrainHeight = terrainHeight,
                            Distance = (float)totalDistance
                        });
                    }
                    else
                    {
                        noDataCount++;
                    }
                }
                else
                {
                    outOfBoundsCount++;
                }

                double distanceStep = _drone.Speed * _updateIntervalSeconds;
                totalDistance += distanceStep;

                (currentLat, currentLon) = CalculateNewPosition(
                    currentLat,
                    currentLon,
                    _drone.Heading,
                    distanceStep);

                currentTime += _updateIntervalSeconds;
                totalSteps++;
            }

            LogService.Log(
                $"=== Симуляция полёта завершена ===\n" +
                $"  Всего шагов: {totalSteps}\n" +
                $"  Точек в треке: {track.Count}\n" +
                $"  Пропусков (вне карты): {outOfBoundsCount}\n" +
                $"  Пропусков (NoData): {noDataCount}\n" +
                $"  Пройденная дистанция: {totalDistance:F2} м\n" +
                $"  Финальная позиция: ({currentLat:F6}, {currentLon:F6})");

            return track;
        }

        private static (double latitude, double longitude) CalculateNewPosition(
            double startLat,
            double startLon,
            double headingDegrees,
            double distanceMeters)
        {
            double headingRadians = headingDegrees * Math.PI / 180.0;

            const double metersPerDegreeLat = 111320.0;
            double metersPerDegreeLon = 111320.0 * Math.Cos(startLat * Math.PI / 180.0);

            double deltaLat = distanceMeters * Math.Cos(headingRadians) / metersPerDegreeLat;
            double deltaLon = distanceMeters * Math.Sin(headingRadians) / metersPerDegreeLon;

            return (startLat + deltaLat, startLon + deltaLon);
        }
    }
}