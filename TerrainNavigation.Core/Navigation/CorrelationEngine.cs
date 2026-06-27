using System;
using System.Collections.Generic;
using System.Text;
using TerrainNavigation.Core.Models;

namespace TerrainNavigation.Core.Navigation
{
    /// <summary>
    /// Correlation Engine (движок корреляции) версии 2.
    /// Поддерживает поиск по 2D позиции и направлению (azimuth — азимут).
    /// </summary>
    public sealed class CorrelationEngine
    {
        private readonly TerrainMap _map;
        List<TerrainPoint> StartPosition;

        bool haveStartPosition = false;

        public double[,,] Heatmap;
        // [row, col, heading]

        public CorrelationEngine(TerrainMap map, List<TerrainPoint> startPosition)
        {
            _map = map;
            StartPosition = startPosition;
            if (StartPosition.Count!=0)
            {
                haveStartPosition = true;
            }

            Heatmap = new double[_map.Rows, _map.Columns, 360];
        }

        /// <summary>
        /// Главный метод поиска позиции и направления.
        /// </summary>
        public (int row, int col, int heading, double bestScore) FindBestMatch2DWithHeading(float[] measuredProfile)
        {
            double bestScore = double.MinValue;
            int bestRow = 0;
            int bestCol = 0;
            int bestHeading = 0;

            int len = measuredProfile.Length;

            for (int r = 10; r < _map.Rows - 10; r++)
            {
                for (int c = 10; c < _map.Columns - 10; c++)
                {
                    for (int h = 0; h < 360; h += 2) // шаг 2° для скорости
                    {
                        float[] synthetic = BuildProfile(r, c, h, len);

                        double score = ComputeCorrelation(measuredProfile, synthetic);

                        Heatmap[r, c, h] = score;

                        if (score > bestScore)
                        {
                            bestScore = score;
                            bestRow = r;
                            bestCol = c;
                            bestHeading = h;
                        }
                    }
                }
            }

            return (bestRow, bestCol, bestHeading, bestScore);
        }

        /// <summary>
        /// Строим "синтетический профиль" рельефа вдоль направления.
        /// (Ray marching — трассировка луча по рельефу)
        /// </summary>
        private float[] BuildProfile(int row, int col, int headingDeg, int length)
        {
            float[] profile = new float[length];

            double rad = headingDeg * Math.PI / 180.0;

            double dRow = Math.Sin(rad);
            double dCol = Math.Cos(rad);

            double r = row;
            double c = col;

            for (int i = 0; i < length; i++)
            {
                int rr = (int)r;
                int cc = (int)c;

                if (!_map.IsInside(rr, cc))
                    break;

                profile[i] = _map.Heights[rr, cc].Height;

                r += dRow;
                c += dCol;
            }

            return profile;
        }

        /// <summary>
        /// Корреляция Пирсона (Pearson correlation — коэффициент корреляции).
        /// </summary>
        private double ComputeCorrelation(float[] a, float[] b)
        {
            int n = Math.Min(a.Length, b.Length);

            double sumA = 0, sumB = 0;
            double sumAB = 0;
            double sumA2 = 0, sumB2 = 0;

            for (int i = 0; i < n; i++)
            {
                double x = a[i];
                double y = b[i];

                sumA += x;
                sumB += y;
                sumAB += x * y;
                sumA2 += x * x;
                sumB2 += y * y;
            }

            double numerator = n * sumAB - sumA * sumB;
            double denominator = Math.Sqrt(
                (n * sumA2 - sumA * sumA) *
                (n * sumB2 - sumB * sumB)
            );

            if (denominator == 0)
                return 0;

            return numerator / denominator;
        }
    }
}
