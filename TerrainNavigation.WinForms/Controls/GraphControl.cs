using TerrainNavigation.Core.Models;

public sealed class GraphControl : Control
{
    private IList<TerrainPoint> _points = Array.Empty<TerrainPoint>();

    public GraphControl()
    {
        DoubleBuffered = true;
        BackColor = Color.Black;
    }

    public void SetPoints(IList<TerrainPoint> points)
    {
        _points = points ?? Array.Empty<TerrainPoint>();
        this.Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        if (_points == null || _points.Count < 2)
        {
            return;
        }

        var g = e.Graphics;

        // Находим минимальные и максимальные значения для масштабирования
        double minHeight = double.MaxValue;
        double maxHeight = double.MinValue;

        foreach (var point in _points)
        {
            if (point == null) continue;

            if (point.Height < minHeight) minHeight = point.Height;
            if (point.Height > maxHeight) maxHeight = point.Height;
        }

        // Добавляем небольшой отступ сверху и снизу (10% от диапазона)
        double heightRange = maxHeight - minHeight;
        if (heightRange < 0.001) heightRange = 1; // Избегаем деления на ноль

        minHeight -= heightRange * 0.1;
        maxHeight += heightRange * 0.1;
        heightRange = maxHeight - minHeight;

        // Размеры области отрисовки с отступами для осей
        int marginLeft = 50;
        int marginBottom = 30;
        int marginRight = 20;
        int marginTop = 20;

        int drawWidth = ClientSize.Width - marginLeft - marginRight;
        int drawHeight = ClientSize.Height - marginBottom - marginTop;

        if (drawWidth <= 0 || drawHeight <= 0) return;

        // Рисуем оси координат
        using var axisPen = new Pen(Color.Gray, 1);
        using var gridPen = new Pen(Color.DarkGray, 1) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dot };
        using var textBrush = new SolidBrush(Color.White);
        using var font = new Font("Arial", 8);

        // Ось X
        g.DrawLine(axisPen, marginLeft, ClientSize.Height - marginBottom,
                   ClientSize.Width - marginRight, ClientSize.Height - marginBottom);
        // Ось Y
        g.DrawLine(axisPen, marginLeft, marginTop,
                   marginLeft, ClientSize.Height - marginBottom);

        // Рисуем метки на оси Y (5 делений)
        for (int i = 0; i <= 4; i++)
        {
            double value = minHeight + (heightRange * i / 4.0);
            float y = ClientSize.Height - marginBottom - (drawHeight * i / 4.0f);

            // Горизонтальная линия сетки
            g.DrawLine(gridPen, marginLeft, y, ClientSize.Width - marginRight, y);

            // Метка значения
            g.DrawString(value.ToString("F1"), font, textBrush, 5, y - 6);
        }

        // Рисуем график
        using var graphPen = new Pen(Color.Lime, 2);

        for (int i = 1; i < _points.Count; i++)
        {
            var first = _points[i - 1];
            var second = _points[i];

            if (first == null || second == null) continue;

            // Масштабируем координаты X
            float x1 = marginLeft + (drawWidth * (i - 1) / (float)(_points.Count - 1));
            float x2 = marginLeft + (drawWidth * i / (float)(_points.Count - 1));

            // Масштабируем координаты Y (инвертируем, т.к. ось Y направлена вниз)
            float y1 = ClientSize.Height - marginBottom - (drawHeight * (float)((first.Height - minHeight) / heightRange));
            float y2 = ClientSize.Height - marginBottom - (drawHeight * (float)((second.Height - minHeight) / heightRange));

            g.DrawLine(graphPen, x1, y1, x2, y2);
        }

        // Метка оси X
        g.DrawString($"Точек: {_points.Count}", font, textBrush, marginLeft, ClientSize.Height - 15);
    }
}