using System;
using System.Collections.Generic;
using System.Text;
using TerrainNavigation.Core.Models;

namespace TerrainNavigation.WinForms.Controls       
{
    /// <summary>
    /// Контрол тепловой карты рельефа.
    /// Отображает TerrainMap (карта рельефа) в виде цветового изображения.
    /// </summary>
    public sealed class HeatMapControl : Control
    {
        private TerrainMap? _map;
        private Bitmap? _bitmap;

        /// <summary>
        /// Устанавливает карту для отображения.
        /// </summary>
        public void SetMap(TerrainMap map)
        {
            _map = map;
            GenerateBitmap();
            Invalidate();
        }

        /// <summary>
        /// Генерация Bitmap (растрового изображения) из карты высот.
        /// </summary>
        private void GenerateBitmap()
        {
            if (_map == null)
                return;

            int width = _map.Columns;
            int height = _map.Rows;

            _bitmap = new Bitmap(width, height);

            float min = _map.MinHeight;
            float max = _map.MaxHeight;
            float range = max - min;

            if (range <= 0)
                range = 1;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float h = _map.Heights[y, x].Height;

                    if (float.IsNaN(h))
                    {
                        _bitmap.SetPixel(x, y, Color.Black);
                        continue;
                    }

                    float t = (h - min) / range;

                    Color color = GetColor(t);

                    _bitmap.SetPixel(x, y, color);
                }
            }
        }

        /// <summary>
        /// Преобразование нормализованного значения (0..1) в цвет.
        /// </summary>
        private Color GetColor(float t)
        {
            // синий -> зелёный -> красный

            if (t < 0.5f)
            {
                float k = t / 0.5f;
                return Color.FromArgb(
                    0,
                    (int)(255 * k),
                    (int)(255 * (1 - k))
                );
            }
            else
            {
                float k = (t - 0.5f) / 0.5f;
                return Color.FromArgb(
                    (int)(255 * k),
                    (int)(255 * (1 - k)),
                    0
                );
            }
        }

        /// <summary>
        /// Отрисовка контрола.
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (_bitmap == null)
            {
                e.Graphics.Clear(Color.Black);
                return;
            }

            e.Graphics.InterpolationMode =
                System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;

            e.Graphics.DrawImage(_bitmap, new Rectangle(0, 0, Width, Height));
        }
    }
}
