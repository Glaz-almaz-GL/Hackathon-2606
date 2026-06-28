using System.Drawing.Drawing2D;
using System.Numerics;
using TerrainScanner.Core.Services;

namespace TerrainScanner.WinForms.Components
{
    public sealed class GraphControl : Control
    {
        /// <summary>
        /// Путь нарисованный пользователем
        /// </summary>
        private readonly List<PointF> _originalPath = [];
        private readonly List<PointF> _secondPath = [];

        public GraphControl()
        {
            DoubleBuffered = true;

            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint |
                     ControlStyles.ResizeRedraw, true);

            BackColor = Color.Black;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            // Включаем сглаживание для красивых линий
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(BackColor);

            if (_originalPath.Count == 0 && _secondPath.Count == 0)
            {
                DrawNoDataMessage(g);
                return;
            }

            // 1. Находим общие границы (Bounding Box) для обоих наборов данных,
            // чтобы они отображались в одном масштабе.
            float minX = float.MaxValue, maxX = float.MinValue;
            float minY = float.MaxValue, maxY = float.MinValue;

            void UpdateBounds(PointF p)
            {
                if (p.X < minX)
                {
                    minX = p.X;
                }

                if (p.X > maxX)
                {
                    maxX = p.X;
                }

                if (p.Y < minY)
                {
                    minY = p.Y;
                }

                if (p.Y > maxY)
                {
                    maxY = p.Y;
                }
            }

            foreach (PointF p in _originalPath)
            {
                UpdateBounds(p);
            }

            foreach (PointF p in _secondPath)
            {
                UpdateBounds(p);
            }

            // Защита от деления на ноль, если все точки имеют одинаковую координату
            if (maxX - minX < float.Epsilon) { maxX += 1f; minX -= 1f; }
            if (maxY - minY < float.Epsilon) { maxY += 1f; minY -= 1f; }

            // 2. Определяем область рисования с отступами для осей и подписей
            float paddingLeft = 50f;  // Место для подписей Y
            float paddingRight = 20f;
            float paddingTop = 20f;
            float paddingBottom = 30f; // Место для подписей X

            float drawWidth = Width - paddingLeft - paddingRight;
            float drawHeight = Height - paddingTop - paddingBottom;

            if (drawWidth <= 0 || drawHeight <= 0)
            {
                return;
            }

            // 3. Функция преобразования координат данных в экранные координаты
            PointF Map(PointF p)
            {
                float nx = (p.X - minX) / (maxX - minX);
                float ny = (p.Y - minY) / (maxY - minY);

                float screenX = paddingLeft + (nx * drawWidth);
                // Инверсия Y, так как в WinForms ось Y направлена вниз
                float screenY = paddingTop + drawHeight - (ny * drawHeight);
                return new PointF(screenX, screenY);
            }

            // 4. Рисуем сетку, рамку и подписи осей
            DrawGridAndAxes(g, minX, maxX, minY, maxY, paddingLeft, paddingTop, drawWidth, drawHeight);

            // 5. Рисуем сами графики
            DrawPath(g, _originalPath, Color.LimeGreen, Map);
            DrawPath(g, _secondPath, Color.OrangeRed, Map);

            // 6. Рисуем легенду
            DrawLegend(g, paddingLeft + 10, paddingTop + 10);
        }

        #region Draw Helpers

        private void DrawNoDataMessage(Graphics g)
        {
            using var font = new Font("Arial", 12f, FontStyle.Italic);
            var text = "Нет данных для отображения";
            SizeF size = g.MeasureString(text, font);
            g.DrawString(text, font, Brushes.Gray,
                (Width - size.Width) / 2,
                (Height - size.Height) / 2);
        }

        private static void DrawGridAndAxes(Graphics g, float minX, float maxX, float minY, float maxY,
            float paddingLeft, float paddingTop, float drawWidth, float drawHeight)
        {
            int gridLines = 5;
            using var gridPen = new Pen(Color.FromArgb(60, 255, 255, 255), 1) { DashStyle = DashStyle.Dot };
            using var axisPen = new Pen(Color.DarkGray, 1);
            using var font = new Font("Consolas", 8f);

            // Рамка области графика
            g.DrawRectangle(axisPen, paddingLeft, paddingTop, drawWidth, drawHeight);

            for (int i = 0; i <= gridLines; i++)
            {
                float ratio = i / (float)gridLines;

                // Вертикальные линии сетки
                float x = paddingLeft + (ratio * drawWidth);
                g.DrawLine(gridPen, x, paddingTop, x, paddingTop + drawHeight);

                // Подписи для оси X (снизу)
                float xVal = minX + (ratio * (maxX - minX));
                var xText = xVal.ToString("F1");
                SizeF xSize = g.MeasureString(xText, font);
                g.DrawString(xText, font, Brushes.Gray, x - (xSize.Width / 2), paddingTop + drawHeight + 5);

                // Горизонтальные линии сетки
                float y = paddingTop + (ratio * drawHeight);
                g.DrawLine(gridPen, paddingLeft, y, paddingLeft + drawWidth, y);

                // Подписи для оси Y (слева)
                // ratio=0 -> верх графика (maxY), ratio=1 -> низ графика (minY)
                float yVal = maxY - (ratio * (maxY - minY));
                var yText = yVal.ToString("F1");
                SizeF ySize = g.MeasureString(yText, font);
                g.DrawString(yText, font, Brushes.Gray, paddingLeft - ySize.Width - 5, y - (ySize.Height / 2));
            }
        }

        private static void DrawPath(Graphics g, List<PointF> path, Color color, Func<PointF, PointF> mapFunc)
        {
            if (path.Count == 0)
            {
                return;
            }

            // Если точка всего одна, рисуем её как круг
            if (path.Count == 1)
            {
                PointF p = mapFunc(path[0]);
                using var brush = new SolidBrush(color);
                g.FillEllipse(brush, p.X - 3, p.Y - 3, 6, 6);
                return;
            }

            // Массовое преобразование координат и отрисовка ломаной линии
            using var pen = new Pen(color, 2f);
            var points = new PointF[path.Count];
            for (int i = 0; i < path.Count; i++)
            {
                points[i] = mapFunc(path[i]);
            }
            g.DrawLines(pen, points);
        }

        private static void DrawLegend(Graphics g, float x, float y)
        {
            using var font = new Font("Arial", 8f);
            float lineHeight = 15f;

            using (var brush = new SolidBrush(Color.LimeGreen))
            {
                g.FillRectangle(brush, x, y, 15, 2);
                g.DrawString("Карта", font, Brushes.White, x + 20, y - 5);
            }

            using (var brush = new SolidBrush(Color.OrangeRed))
            {
                g.FillRectangle(brush, x, y + lineHeight, 15, 2);
                g.DrawString("Высотомер", font, Brushes.White, x + 20, y + lineHeight - 5);
            }
        }

        #endregion

        #region Draw Methods

        /// <summary>
        /// Обновляет данные для графиков и инициирует перерисовку контрола.
        /// Метод потокобезопасен (поддерживает вызов из фоновых потоков).
        /// </summary>
        public void DrawGraphs(IEnumerable<Vector2> originalPath, IEnumerable<Vector2> secondPath)
        {
            try
            {
                _originalPath.Clear();
                _secondPath.Clear();

                if (originalPath != null)
                {
                    foreach (Vector2 p in originalPath)
                    {
                        _originalPath.Add(new PointF(p.X, p.Y));
                    }
                }

                if (secondPath != null)
                {
                    foreach (Vector2 p in secondPath)
                    {
                        _secondPath.Add(new PointF(p.X, p.Y));
                    }
                }

                LogService.Log($"GraphControl: Данные обновлены. Карта: {_originalPath.Count} точек, Высотомер: {_secondPath.Count} точек");

                // Потокобезопасный вызов перерисовки
                if (InvokeRequired)
                {
                    Invoke(new Action(() => Invalidate()));
                }
                else
                {
                    Invalidate();
                }
            }
            catch (Exception ex)
            {
                LogService.Log($"ОШИБКА GraphControl: Не удалось обновить данные графика: {ex.Message}");
            }
        }



        #endregion
    }
}