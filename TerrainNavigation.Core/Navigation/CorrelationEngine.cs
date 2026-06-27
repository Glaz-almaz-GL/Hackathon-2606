using TerrainNavigation.Core.Models;

public sealed class CorrelationEngine
{
    private readonly TerrainMap _map;
    private readonly List<TerrainPoint> _startPosition;
    private readonly bool _haveStartPosition = false;

    public CorrelationEngine(TerrainMap map, List<TerrainPoint> startPosition)
    {
        _map = map;
        _startPosition = startPosition;
        if (_startPosition != null && _startPosition.Count != 0)
        {
            _haveStartPosition = true;
        }
    }

    public (int row, int col, int heading, double bestScore) FindBestMatch2DWithHeading(float[] measuredProfile)
    {
        double bestScore = double.MinValue;
        int bestRow = 0;
        int bestCol = 0;
        int bestHeading = 0;

        int len = measuredProfile.Length;

        // Создаем ОДИН буфер для профиля и переиспользуем его миллиарды раз, не перегружая GC
        float[] syntheticBuffer = new float[len];

        for (int r = 10; r < _map.Rows - 10; r++)
        {
            for (int c = 10; c < _map.Columns - 10; c++)
            {
                for (int h = 0; h < 360; h += 2)
                {
                    // Передаем буфер внутрь для заполнения без выделения new float[]
                    int actualLength = FillProfile(r, c, h, syntheticBuffer);

                    // Считаем корреляцию только по фактически заполненной длине луча
                    double score = ComputeCorrelation(measuredProfile, syntheticBuffer, actualLength);

                    // УДАЛЕНО: Heatmap[r, c, h] = score;

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
    /// Изменено: Заполняет готовый буфер вместо аллокации нового массива
    /// </summary>
    private int FillProfile(int row, int col, int headingDeg, float[] buffer)
    {
        double rad = headingDeg * Math.PI / 180.0;
        double dRow = Math.Sin(rad);
        double dCol = Math.Cos(rad);

        double r = row;
        double c = col;

        int i = 0;
        for (; i < buffer.Length; i++)
        {
            int rr = (int)r;
            int cc = (int)c;

            if (!_map.IsInside(rr, cc))
                break; // Луч вышел за границы карты

            buffer[i] = _map.Heights[rr, cc].Height;

            r += dRow;
            c += dCol;
        }
        return i; // Возвращаем сколько точек луча реально удалось построить
    }

    /// <summary>
    /// Изменено: учитывает эффективную длину массивов
    /// </summary>
    private double ComputeCorrelation(float[] a, float[] b, int effectiveLength)
    {
        int n = Math.Min(a.Length, effectiveLength);
        if (n < 2) return 0; // Для коррекции нужно хотя бы 2 точки

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
