using System;
using System.Collections.Generic;
using System.Text;

namespace TerrainNavigation.WinForms.Controls
{
    /// <summary>
    /// Контрол рисования траектории мышью.
    /// </summary>
    public sealed class FlightPathControl : Control
    {
        public List<Point> Points { get; } = new();

        private bool _drawing;

        public event Action? PathChanged;

        public FlightPathControl()
        {
            DoubleBuffered = true;
            BackColor = Color.Black;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            _drawing = true;
            Points.Clear();
            Points.Add(e.Location);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (!_drawing) return;

            Points.Add(e.Location);
            PathChanged?.Invoke();
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            _drawing = false;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (Points.Count < 2) return;

            using var pen = new Pen(Color.Lime, 2);

            for (int i = 1; i < Points.Count; i++)
            {
                e.Graphics.DrawLine(pen, Points[i - 1], Points[i]);
            }
        }
    }
}
