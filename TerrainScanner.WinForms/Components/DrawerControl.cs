using Microsoft.Extensions.Logging;
using System.Numerics;
using TerrainScanner.Core.Models;
using TerrainScanner.Core.Models.Models;
using TerrainScanner.Core.Models.Terrain;

namespace TerrainScanner.WinForms.Components
{
    public sealed class DrawerControl : Control
    {
        private readonly ILogger<DrawerControl>? _logger;

        private Bitmap? _terrainBitmap;

        private PointF _straightLineStart;
        private PointF _straightLineEnd;
        private bool _showStraightLine = false;

        /// <summary>
        /// Путь дрона, полученный в результате симуляции (в экранных координатах)
        /// </summary>
        private readonly List<PointF> _dronePath = [];

        /// <summary>
        /// Путь нарисованный пользователем
        /// </summary>
        private readonly List<PointF> _drawPoints = [];
        private bool _isDrawing = false;

        private TerrainMap? _map;

        public DrawerControl(ILogger<DrawerControl>? logger = null, TerrainMap? map = null)
        {
            _logger = logger;
            DoubleBuffered = true;

            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint |
                     ControlStyles.ResizeRedraw, true);

            BackColor = Color.Black;
            _map = map;
        }

        public void SetMap(TerrainMap map)
        {
            _map?.Dispose();
            _map = map;

            BuildTerrainBitmap();
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;

            g.Clear(Color.Black);

            DrawTerrain(g);
            DrawUserPath(g);
            DrawDronePath(g);

            if (_showStraightLine)
            {
                DrawStraightLineOverlay(g);
            }
        }

        public List<Vector2> GetUserPathAs2D()
        {
            var path = GetUserPath();
            if (path.Count == 0) return [];

            var profile = new List<Vector2>(path.Count);
            float totalDistance = 0f;

            // Первая точка — начало отсчета (дистанция 0)
            profile.Add(new Vector2(0f, path[0].Height));

            for (int i = 1; i < path.Count; i++)
            {
                var prev = path[i - 1];
                var curr = path[i];

                // Разница в градусах
                double dLat = curr.Latitude - prev.Latitude;
                double dLon = curr.Longitude - prev.Longitude;

                // Переводим дельты в метры.
                // Для X используем среднюю широту между точками для большей точности.
                double avgLat = (prev.Latitude + curr.Latitude) / 2.0;
                double distY = TerrainMap.LatitudeDeltaToMeters(dLat);
                double distX = TerrainMap.LongitudeDeltaToMeters(dLon, avgLat);

                // Расстояние по теореме Пифагора (для малых расстояний между соседними точками
                double segmentDistance = Math.Sqrt(distX * distX + distY * distY);

                totalDistance += (float)segmentDistance;

                // Добавляем точку профиля: (Дистанция, Высота)
                profile.Add(new Vector2(totalDistance, curr.Height));
            }

            return profile;
        }

        #region Helper Methods

        /// <summary>
        /// Построение bitmap рельефа (делается 1 раз)
        /// </summary>
        private void BuildTerrainBitmap()
        {
            if (_map == null)
            {
                return;
            }

            _logger?.LogInformation("Building Bitmap for GeoTiff data");

            _terrainBitmap?.Dispose();
            _terrainBitmap = _map.ToBitmap();
        }

        public List<TerrainPoint> GetUserPath()
        {
            return GetUserPath(_drawPoints);
        }

        public List<TerrainPoint> GetUserPath(IList<PointF> drawPoints)
        {
            if (_map is null)
            {
                return [];
            }

            if (drawPoints.Count == 0)
            {
                return [];
            }

            HashSet<(int Row, int Col)> processedCells = [];
            List<TerrainPoint> terrainPoints = [];

            int width = _map.Columns;
            int height = _map.Rows;

            // Масштаб: сколько колонок/строк приходится на один пиксель UI
            double scaleX = (double)width / this.Width;
            double scaleY = (double)height / this.Height;

            // 3. Интерполяция и обход точек
            for (int i = 0; i < drawPoints.Count; i++)
            {
                PointF current = drawPoints[i];

                double currentCol = current.X * scaleX;
                double currentRow = current.Y * scaleY;

                // Если это не первая точка, интерполируем отрезок от предыдущей точки
                if (i > 0)
                {
                    PointF previous = drawPoints[i - 1];
                    double prevCol = previous.X * scaleX;
                    double prevRow = previous.Y * scaleY;

                    // Интерполяция (простой DDA-алгоритм)
                    int steps = (int)Math.Max(Math.Abs(currentCol - prevCol), Math.Abs(currentRow - prevRow));
                    if (steps > 0)
                    {
                        double stepCol = (currentCol - prevCol) / steps;
                        double stepRow = (currentRow - prevRow) / steps;

                        for (int s = 0; s <= steps; s++)
                        {
                            int col = (int)(prevCol + (s * stepCol));
                            int row = (int)(prevRow + (s * stepRow));

                            ProcessCell(row, col, ref processedCells, ref terrainPoints);
                        }
                    }
                }
                else
                {
                    // Обработка самой первой точки
                    ProcessCell((int)currentRow, (int)currentCol, ref processedCells, ref terrainPoints);
                }
            }

            return terrainPoints;
        }

        private void ProcessCell(int row, int col, ref HashSet<(int Row, int Col)> processedCells, ref List<TerrainPoint> terrainPoints)
        {
            if (_map == null)
            {
                return;
            }

            if (!_map.IsInside(row, col))
            {
                return;
            }

            // HashSet гарантирует, что мы не добавим дубликаты
            if (!processedCells.Add((row, col)))
            {
                return;
            }

            float pointHeight = _map.GetHeight(row, col);

            // Пропускаем "дыры" в рельефе (NaN), если это требуется по логике
            if (float.IsNaN(pointHeight))
            {
                return;
            }

            terrainPoints.Add(new TerrainPoint(
                _map.GetLongitude(col),
                _map.GetLatitude(row),
                pointHeight
            ));
        }
        #endregion

        #region Draw Methods

        /// <summary>
        /// Рисуем рельеф
        /// </summary>
        private void DrawTerrain(Graphics g)
        {
            if (_terrainBitmap == null)
            {
                return;
            }

            _logger?.LogInformation("Drwaing terrain for GeoTiff data");

            g.InterpolationMode =
                System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;

            g.DrawImage(_terrainBitmap,
                new Rectangle(0, 0, Width, Height));
        }

        /// <summary>
        /// Рисуем траекторию пользователя
        /// </summary>
        private void DrawUserPath(Graphics g)
        {
            if (_drawPoints.Count < 2)
            {
                return;
            }

            using Pen pen = new(Color.Red, 2);

            for (int i = 1; i < _drawPoints.Count; i++)
            {
                g.DrawLine(pen,
                    _drawPoints[i - 1],
                    _drawPoints[i]);
            }

            // точка старта
            g.FillEllipse(Brushes.Lime, _drawPoints[0].X - 3, _drawPoints[0].Y - 3, 6, 6);

            // текущая точка
            PointF last = _drawPoints[^1];
            g.FillEllipse(Brushes.Yellow, last.X - 3, last.Y - 3, 6, 6);
        }

        /// <summary>
        /// Рисуем траекторию полета дрона
        /// </summary>
        private void DrawDronePath(Graphics g)
        {
            if (_dronePath.Count == 0)
            {
                return;
            }

            // Используем Cyan (голубой) для отличия от красного пути пользователя
            using Pen pen = new(Color.Cyan, 2f)
            {
                DashStyle = System.Drawing.Drawing2D.DashStyle.Dash // Пунктир для визуального отличия
            };

            // Если точка всего одна — рисуем её маркером
            if (_dronePath.Count == 1)
            {
                g.FillEllipse(Brushes.Cyan, _dronePath[0].X - 4, _dronePath[0].Y - 4, 8, 8);
                return;
            }

            // Рисуем линию траектории
            g.DrawLines(pen, [.. _dronePath]);

            // Маркер старта дрона (большой круг)
            g.FillEllipse(Brushes.Cyan, _dronePath[0].X - 5, _dronePath[0].Y - 5, 10, 10);
            g.DrawEllipse(Pens.White, _dronePath[0].X - 5, _dronePath[0].Y - 5, 10, 10);

            // Маркер текущей позиции дрона (последняя точка)
            PointF droneCurrent = _dronePath[^1];
            g.FillEllipse(Brushes.White, droneCurrent.X - 4, droneCurrent.Y - 4, 8, 8);
            g.DrawEllipse(Pens.Cyan, droneCurrent.X - 4, droneCurrent.Y - 4, 8, 8);
        }
        #endregion

        #region Drone
        /// <summary>
        /// Устанавливает путь дрона для отрисовки на карте.
        /// Ожидает точки в географических координатах (Latitude, Longitude).
        /// </summary>
        public void SetDronePath(IEnumerable<DroneTrackPoint> droneGeoPath)
        {
            _dronePath.Clear();

            if (_map is null || droneGeoPath is null)
            {
                Invalidate();
                return;
            }

            // Преобразуем географические координаты в экранные
            double scaleX = (double)Width / _map.Columns;
            double scaleY = (double)Height / _map.Rows;

            foreach (var point in droneGeoPath)
            {
                int col = _map.GetColumn(point.Longitude);
                int row = _map.GetRow(point.Latitude);

                if (_map.IsInside(row, col))
                {
                    // Переводим индексы сетки в пиксели UI
                    float screenX = (float)(col * scaleX);
                    float screenY = (float)(row * scaleY);
                    _dronePath.Add(new PointF(screenX, screenY));
                }
            }

            _logger?.LogDebug("Drone path set. Points count: {Count}", _dronePath.Count);
            Invalidate();
        }

        /// <summary>
        /// Устанавливает путь дрона напрямую в экранных координатах.
        /// </summary>
        public void SetDronePathScreenSpace(IEnumerable<PointF> droneScreenPath)
        {
            _dronePath.Clear();
            if (droneScreenPath is not null)
            {
                _dronePath.AddRange(droneScreenPath);
            }
            Invalidate();
        }

        /// <summary>
        /// Рисует вспомогательную прямую линию между двумя точками
        /// </summary>
        private void DrawStraightLineOverlay(Graphics g)
        {
            using Pen pen = new(Color.Magenta, 2f)
            {
                DashStyle = System.Drawing.Drawing2D.DashStyle.DashDot
            };

            g.DrawLine(pen, _straightLineStart, _straightLineEnd);

            // Маркеры на концах линии
            g.FillEllipse(Brushes.Magenta, _straightLineStart.X - 4, _straightLineStart.Y - 4, 8, 8);
            g.FillEllipse(Brushes.Magenta, _straightLineEnd.X - 4, _straightLineEnd.Y - 4, 8, 8);
        }
        #endregion

        #region INPUT (mouse)

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            _isDrawing = true;
            _drawPoints.Clear();

            _drawPoints.Add(e.Location);

            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (!_isDrawing)
            {
                return;
            }

            _drawPoints.Add(e.Location);

            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            _isDrawing = false;
        }

        #endregion

        #region Dispose
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _terrainBitmap?.Dispose();
            }

            base.Dispose(disposing);
        }
        #endregion

        #region StraightPath
        /// <summary>
        /// Заменяет нарисованный путь прямой линией между начальной и конечной точками.
        /// Полезно для создания ровной траектории для симуляции полета дрона.
        /// </summary>
        /// <param name="pointSpacing">Расстояние между точками в пикселях (по умолчанию 5)</param>
        public void StraightenPath(int pointSpacing = 5)
        {
            if (_drawPoints.Count < 2)
            {
                _logger?.LogWarning("Невозможно выровнять путь: нужно минимум 2 точки");
                return;
            }

            // Получаем начальную и конечную точки
            PointF start = _drawPoints[0];
            PointF end = _drawPoints[^1];

            // Вычисляем расстояние между точками
            double dx = end.X - start.X;
            double dy = end.Y - start.Y;
            double distance = Math.Sqrt(dx * dx + dy * dy);

            // Количество промежуточных точек
            int numPoints = (int)(distance / pointSpacing);
            if (numPoints < 2) numPoints = 2;

            // Создаем новую коллекцию точек по прямой линии
            List<PointF> straightPath = new(numPoints);

            for (int i = 0; i <= numPoints; i++)
            {
                double t = (double)i / numPoints;
                float x = (float)(start.X + t * dx);
                float y = (float)(start.Y + t * dy);
                straightPath.Add(new PointF(x, y));
            }

            // Заменяем старый путь на новый
            _drawPoints.AddRange(straightPath);

            _logger?.LogInformation(
                "Путь выровнен. Начальная точка: ({X1:F1}, {Y1:F1}), Конечная точка: ({X2:F1}, {Y2:F1}), Точек: {Count}",
                start.X, start.Y, end.X, end.Y, straightPath.Count);

            Invalidate();
        }

        /// <summary>
        /// Рисует прямую линию между двумя точками пути без изменения самих точек.
        /// </summary>
        /// <param name="startIndex">Индекс начальной точки</param>
        /// <param name="endIndex">Индекс конечной точки</param>
        public void DrawStraightLineBetween(int startIndex, int endIndex)
        {
            if (_drawPoints.Count < 2)
            {
                _logger?.LogWarning("Невозможно нарисовать линию: нужно минимум 2 точки");
                return;
            }

            if (startIndex < 0 || startIndex >= _drawPoints.Count ||
                endIndex < 0 || endIndex >= _drawPoints.Count)
            {
                _logger?.LogWarning("Индексы точек выходят за пределы диапазона");
                return;
            }

            // Сохраняем точки для отрисовки прямой линии
            _straightLineStart = _drawPoints[startIndex];
            _straightLineEnd = _drawPoints[endIndex];
            _showStraightLine = true;

            _logger?.LogDebug(
                "Прямая линия между точками [{Index1}] и [{Index2}]",
                startIndex, endIndex);

            Invalidate();
        }

        /// <summary>
        /// Скрывает вспомогательную прямую линию
        /// </summary>
        public void HideStraightLine()
        {
            _showStraightLine = false;
            Invalidate();
        }
        #endregion
    }
}
