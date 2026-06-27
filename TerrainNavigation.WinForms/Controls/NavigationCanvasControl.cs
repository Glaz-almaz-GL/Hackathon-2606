using TerrainNavigation.Core.Models;
using TerrainNavigation.Core.Navigation;
using TerrainNavigation.Core.Services;

namespace TerrainNavigation.WinForms.Controls
{
    /// <summary>
    /// Единый canvas-контрол:
    /// - отображает карту рельефа
    /// - принимает ввод мышью (траектория)
    /// - готов для heatmap и симуляции
    /// </summary>
    public class NavigationCanvasControl : Control
    {
        private TerrainMap? _map;
        private Bitmap? _terrainBitmap;
        private CorrelationEngine? _engine;
        private FlightPath? _path;

        private double _noiseLevel = 0;

        // Точки, которые рисует пользователь (true path)
        private readonly List<PointF> _drawPoints = [];

        //Восстановленный путь
        private readonly List<PointF> _estimatedPoints = [];


        private bool _isDrawing = false;

        public NavigationCanvasControl()
        {
            DoubleBuffered = true;

            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint |
                     ControlStyles.ResizeRedraw, true);

            BackColor = Color.Black;
        }

        #region MAP

        /// <summary>
        /// Установить карту рельефа и построить bitmap
        /// </summary>
        public void SetMap(TerrainMap map)
        {
            _map = map;
            _engine = new CorrelationEngine(map, []);
            BuildTerrainBitmap();

            Invalidate();
        }

        /// <summary>
        /// Построение bitmap рельефа (делается 1 раз)
        /// </summary>
        private void BuildTerrainBitmap()
        {
            if (_map == null)
            {
                return;
            }

            _terrainBitmap?.Dispose();

            _terrainBitmap = new Bitmap(_map.Columns, _map.Rows);

            for (int r = 0; r < _map.Rows; r++)
            {
                for (int c = 0; c < _map.Columns; c++)
                {
                    float h = _map.Heights[r, c].Height;

                    int gray = (int)Math.Clamp(h /*/ 10f*/, 0, 255);

                    _terrainBitmap.SetPixel(c, r, Color.FromArgb(gray, gray, gray));
                }
            }
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

        #region RENDER

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;

            g.Clear(Color.Black);

            DrawTerrain(g);
            DrawUserPath(g);
            DrawGeneratedPath(g);
            DrawEstimatedPath(g);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            if (_map is null)
            {
                return;
            }

            double dx = (_map.MaxLatitude - _map.MinLatitude) * (e.X / (double)this.Width);
            double dy = (_map.MaxLongitude - _map.MinLongitude) * (e.Y / (double)this.Height);
            if (_drawPoints.Count < 10)
            {
                return;
            }

            RunSimulationAndEstimation(_noiseLevel);
        }

        /// <summary>
        /// Рисуем рельеф
        /// </summary>
        private void DrawTerrain(Graphics g)
        {
            if (_terrainBitmap == null)
            {
                return;
            }

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

            using var pen = new Pen(Color.Red, 2);

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
        /// Рисуем сгенерированный путь (FlightPath)
        /// </summary>
        private void DrawGeneratedPath(Graphics g)
        {
            if (_path == null || _path.Points.Count < 2 || _map == null)
            {
                return;
            }

            using var pen = new Pen(Color.Cyan, 2);

            // Конвертируем координаты пути в пиксели экрана
            var screenPoints = new List<PointF>();

            foreach ((double X, double Y) in _path.Points)
            {
                // p.X = Longitude, p.Y = Latitude (в градусах)
                // Конвертируем градусы в пиксели экрана
                float pixelX = (float)((X - _map.MinLongitude) / (_map.MaxLongitude - _map.MinLongitude) * Width);
                float pixelY = (float)((Y - _map.MinLatitude) / (_map.MaxLatitude - _map.MinLatitude) * Height);

                screenPoints.Add(new PointF(pixelX, pixelY));
            }

            // Рисуем линию
            for (int i = 1; i < screenPoints.Count; i++)
            {
                g.DrawLine(pen, screenPoints[i - 1], screenPoints[i]);
            }

            // Точка старта сгенерированного пути
            if (screenPoints.Count > 0)
            {
                g.FillEllipse(Brushes.Cyan, screenPoints[0].X - 4, screenPoints[0].Y - 4, 8, 8);
            }
        }

        /// <summary>
        /// Рисуем восстановленный путь (результат корреляции)
        /// </summary>
        private void DrawEstimatedPath(Graphics g)
        {
            if (_estimatedPoints.Count < 2 || _map == null)
            {
                return;
            }

            using var pen = new Pen(Color.Magenta, 3);

            // _estimatedPoints хранит индексы сетки (col, row)
            // Конвертируем в пиксели экрана
            var screenPoints = new List<PointF>();

            foreach (PointF p in _estimatedPoints)
            {
                // p.X = col, p.Y = row
                float pixelX = p.X / _map.Columns * Width;
                float pixelY = p.Y / _map.Rows * Height;

                screenPoints.Add(new PointF(pixelX, pixelY));
            }

            // Рисуем линию
            for (int i = 1; i < screenPoints.Count; i++)
            {
                g.DrawLine(pen, screenPoints[i - 1], screenPoints[i]);
            }

            // Точка старта восстановленного пути
            if (screenPoints.Count > 0)
            {
                g.FillEllipse(Brushes.Magenta, screenPoints[0].X - 5, screenPoints[0].Y - 5, 10, 10);
            }
        }

        #endregion

        #region API (для будущей симуляции)

        public List<PointF> GetUserPath()
        {
            return new List<PointF>(_drawPoints);
        }

        public void ClearPath()
        {
            _drawPoints.Clear();
            _estimatedPoints.Clear();
            _path = null;
            Invalidate();
        }

        #endregion

        public void SetNoise(double noise)
        {
            _noiseLevel = noise;
        }

        public void ForceRunSimulation(double noise)
        {
            _noiseLevel = noise;

            RunSimulationAndEstimation(_noiseLevel);

            LogService.Log($"Симуляция запущена с уровнем шума = {_noiseLevel}");

            Invalidate();
        }

        /// <summary>
        /// Главный цикл: симуляция → корреляция → восстановление
        /// </summary>
        private void RunSimulationAndEstimation(double noiseMeters)
        {
            if (_map == null)
            {
                LogService.Log("Карта не создана. Симуляция невозможна.");
                return;
            }

            if (_engine == null)
            {
                LogService.Log("Движок не установлен. Симуляция невозможна.");
                return;
            }

            if (_drawPoints.Count < 10)
            {
                LogService.Log("Недостаточно точек для запуска симуляции.");
                return;
            }

            LogService.Log("Симуляция началась. Расчет точек пути");

            // -----------------------------
            // 1. превращаем mouse path в FlightPath
            // -----------------------------
            var path = new FlightPath();

            int width = _map.Columns;
            int height = _map.Rows;



            double dw = (double)width / this.Width;
            double dh = (double)height / this.Height;

            foreach (PointF p in _drawPoints)
            {
                path.Points.Add((p.X * dw, p.Y * dh));
            }

            _path = path;

            LogService.Log("Генерация измерений (радиовысотомер) с шумом: " + noiseMeters + " метров");
            // -----------------------------
            // 2. генерируем измерения (радиовысотомер)
            // -----------------------------
            var generator = new FlightPathGenerator(_map);

            (AircraftState[]? states, double[]? radar) = generator.Generate(path, 1500, _noiseLevel);


            LogService.Log("Построение профиля измерений");
            // -----------------------------
            // 3. строим профиль (measured signal)
            // -----------------------------
            float[] measured = new float[radar.Length];

            for (int i = 0; i < radar.Length; i++)
            {
                measured[i] = (float)radar[i];
            }

            LogService.Log("Длина профиля: " + measured.Length);

            LogService.Log("Расчет корреляции");
            // -----------------------------
            // 4. корреляция (локализация)
            // -----------------------------
            (int row, int col, int heading, double bestScore) = _engine.FindBestMatch2DWithHeading(measured);

            LogService.Log("Расчет корреляции завершен. ");
            LogService.Log($"Лучший результат: row={row}, col={col}, heading={heading}, score={bestScore}");

            LogService.Log("Отрисовка восстановленного пути");
            // -----------------------------
            // 5. восстановленный путь (упрощённо)
            // -----------------------------
            _estimatedPoints.Clear();

            double cx = row;
            double cy = col;

            for (int i = 0; i < states.Length; i++)
            {
                _estimatedPoints.Add(new PointF(
                    (float)(cy + (i * 0.5)),
                    (float)(cx + (i * 0.5))
                ));
            }
            LogService.Log("Отрисовано " + _estimatedPoints.Count + " точек");
            Invalidate(); // Перерисовываем контрол
        }

        /// <summary>
        /// Генерирует случайный прямой маршрут заданной длины (в метрах).
        /// </summary>
        public void GeneratePath(int lengthMeters)
        {
            LogService.Log($"Начало генерации маршрута длиной {lengthMeters} метров");

            if (_map is null)
            {
                LogService.Log("Ошибка: карта (_map) не инициализирована. Генерация маршрута прервана.");
                return;
            }

            Random random = new();

            // Генерация случайных координат в пределах карты
            double startLon = (random.NextDouble() * (_map.MaxLongitude - _map.MinLongitude)) + _map.MinLongitude;
            double startLat = (random.NextDouble() * (_map.MaxLatitude - _map.MinLatitude)) + _map.MinLatitude;

            LogService.Log($"Сгенерирована начальная точка: Lat={startLat:F6}, Lon={startLon:F6}");

            // Для простоты генерируем вторую точку случайно
            double endLon = (random.NextDouble() * (_map.MaxLongitude - _map.MinLongitude)) + _map.MinLongitude;
            double endLat = (random.NextDouble() * (_map.MaxLatitude - _map.MinLatitude)) + _map.MinLatitude;

            LogService.Log($"Сгенерирована конечная точка: Lat={endLat:F6}, Lon={endLon:F6}");

            var path = new FlightPath();

            // Интерполяция пути между точками
            double dLat = endLat - startLat;
            double dLon = endLon - startLon;

            LogService.Log($"Дельта координат: dLat={dLat:F6}, dLon={dLon:F6}");

            // Количество шагов зависит от длины пути и разрешения карты (чтобы не пропускать пиксели)
            // шаг равен половине размера пикселя в метрах, переведенному в градусы
            double stepSizeDeg = Math.Min(Math.Abs(_map.LongitudeStep), Math.Abs(_map.LatitudeStep)) / 2.0;

            LogService.Log($"Размер шага интерполяции: {stepSizeDeg:F8} градусов");

            // Грубая оценка количества шагов по максимальной дельте
            int steps = (int)Math.Ceiling(Math.Max(Math.Abs(dLat), Math.Abs(dLon)) / stepSizeDeg);
            if (steps < 2)
            {
                steps = 2;
            }

            LogService.Log($"Количество шагов интерполяции: {steps}");

            _drawPoints.Clear(); // Очищаем пользовательский путь перед генерацией

            for (int i = 0; i <= steps; i++)
            {
                double t = (double)i / steps;

                // Линейная интерполяция координат
                double currentLat = startLat + (dLat * t);
                double currentLon = startLon + (dLon * t);

                // ВАЖНО: FlightPath.Points ожидает (X, Y), где X=Lon, Y=Lat
                path.Points.Add((currentLon, currentLat));

                // Конвертируем градусы в пиксели экрана для _drawPoints
                float pixelX = (float)((currentLon - _map.MinLongitude) / (_map.MaxLongitude - _map.MinLongitude) * Width);
                float pixelY = (float)((currentLat - _map.MinLatitude) / (_map.MaxLatitude - _map.MinLatitude) * Height);
                _drawPoints.Add(new PointF(pixelX, pixelY));
            }

            _path = path;
            LogService.Log($"Генерация маршрута завершена. Создано точек: {path.Points.Count}");

            Invalidate(); // Перерисовываем контрол
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _terrainBitmap?.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}