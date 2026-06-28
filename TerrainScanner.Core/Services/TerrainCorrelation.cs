using MathNet.Numerics;
using MathNet.Numerics.Interpolation;
using MathNet.Numerics.Statistics;
using System.Numerics;
using TerrainScanner.Core.Models;

namespace TerrainScanner.Core.Services
{
    /// <summary>
    /// Расчёт корреляции между регулярной сеткой (синяя линия)
    /// и нерегулярными замерами (красные точки дрона).
    /// </summary>
    public static class TerrainCorrelation
    {
        /// <summary>
        /// Базовый расчёт корреляции Пирсона.
        /// </summary>
        /// <param name="blueLine">Точки синей линии (регулярная сетка).</param>
        /// <param name="redPoints">Точки красных замеров (замеры дрона).</param>
        /// <param name="method">Метод интерполяции синей линии.</param>
        public static double CalculateCorrelation(
            IEnumerable<Vector2> blueLine,
            IEnumerable<Vector2> redPoints,
            InterpolationMethod method = InterpolationMethod.Linear)
        {
            if (!ValidateInput(blueLine, redPoints)) return 0;

            LogService.Log($"Расчёт корреляции Пирсона: blueLine={blueLine.Count()}, redPoints={redPoints.Count()}, метод={method}");

            IInterpolation interpolator = CreateInterpolator(blueLine, method);
            double[] blueInterpolated = [.. redPoints.Select(p => interpolator.Interpolate(p.X))];
            double[] redY = [.. redPoints.Select(p => p.Y)];

            double result = Correlation.Pearson(blueInterpolated, redY);

            LogService.Log($"Корреляция Пирсона: {result:F4}");
            return result;
        }

        /// <summary>
        /// Расширенный расчёт с набором статистических метрик.
        /// </summary>
        public static CorrelationResult? CalculateCorrelationDetailed(
            IEnumerable<Vector2> blueLine,
            IEnumerable<Vector2> redPoints,
            InterpolationMethod method = InterpolationMethod.Linear)
        {
            if (!ValidateInput(blueLine, redPoints)) return null;

            LogService.Log($"=== Начало детального расчёта корреляции ===");
            LogService.Log($"  Входные данные: blueLine={blueLine.Count()} точек, redPoints={redPoints.Count()} точек");
            LogService.Log($"  Метод интерполяции: {method}");

            IInterpolation interpolator = CreateInterpolator(blueLine, method);
            double[] blueInterpolated = [.. redPoints.Select(p => interpolator.Interpolate(p.X))];
            double[] redY = [.. redPoints.Select(p => p.Y)];

            LogService.Log($"Интерполяция выполнена. Точек после интерполяции: {blueInterpolated.Length}");

            // Корреляция Пирсона (линейная зависимость)
            double pearson = Correlation.Pearson(blueInterpolated, redY);

            // Корреляция Спирмена (монотонная зависимость)
            double spearman = Correlation.Spearman(blueInterpolated, redY);

            // RMSE — среднеквадратичная ошибка
            double rmse = Math.Sqrt(
                blueInterpolated.Zip(redY, (b, r) => Math.Pow(b - r, 2)).Average());

            // MAE — средняя абсолютная ошибка
            double mae = blueInterpolated
                .Zip(redY, (b, r) => Math.Abs(b - r))
                .Average();

            // R² — коэффициент детерминации
            double meanRed = redY.Average();
            double ssTot = redY.Sum(y => Math.Pow(y - meanRed, 2));
            double ssRes = blueInterpolated.Zip(redY, (b, r) => Math.Pow(r - b, 2)).Sum();
            double rSquared = ssTot < (0 + 1e-9) ? 0 : 1 - (ssRes / ssTot);

            LogService.Log(
                $"Результаты расчёта:\n" +
                $"  Pearson: {pearson:F4}\n" +
                $"  Spearman: {spearman:F4}\n" +
                $"  RMSE: {rmse:F2}\n" +
                $"  MAE: {mae:F2}\n" +
                $"  R²: {rSquared:F4}");

            return new CorrelationResult
            {
                PearsonCorrelation = pearson,
                SpearmanCorrelation = spearman,
                RMSE = rmse,
                MAE = mae,
                R2 = rSquared,
                InterpolatedBlueValues = blueInterpolated
            };
        }

        /// <summary>
        /// Валидация входных данных.
        /// </summary>
        private static bool ValidateInput(IEnumerable<Vector2> blueLine, IEnumerable<Vector2> redPoints)
        {
            if (blueLine is null || blueLine.Count() < 2)
            {
                LogService.Log($"ОШИБКА валидации: blueLine содержит недостаточно точек (нужно минимум 2)");
                return false;
            }

            if (redPoints is null || redPoints.Count() < 3)
            {
                LogService.Log($"ОШИБКА валидации: redPoints содержит недостаточно точек (нужно минимум 3)");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Создание интерполятора по массиву точек.
        /// </summary>
        private static IInterpolation CreateInterpolator(
            IEnumerable<Vector2> points, InterpolationMethod method)
        {
            double[] x = [.. points.Select(p => p.X)];
            double[] y = [.. points.Select(p => p.Y)];

            LogService.Log($"Создание интерполятора: точек={points.Count()}, метод={method}");

            return method switch
            {
                InterpolationMethod.Linear => Interpolate.Linear(x, y),
                InterpolationMethod.CubicSpline => Interpolate.CubicSpline(x, y),
                InterpolationMethod.LogLinear => Interpolate.LogLinear(x, y),
                _ => Interpolate.Linear(x, y)
            };
        }
    }
}