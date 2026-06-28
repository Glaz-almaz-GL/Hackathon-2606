using System;
using System.Collections.Generic;
using System.Text;

namespace TerrainScanner.Core.Models
{
    /// <summary>
    /// Представляет результаты статистического анализа и оценки корреляции между двумя наборами данных.
    /// </summary>
    public sealed class CorrelationResult
    {
        /// <summary>
        /// Коэффициент корреляции Пирсона.
        /// Отражает степень линейной статистической связи между двумя наборами данных (от -1 до 1).
        /// </summary>
        public double PearsonCorrelation { get; set; }

        /// <summary>
        /// Коэффициент ранговой корреляции Спирмена.
        /// Оценивает монотонность связи между переменными, устойчив к выбросам (от -1 до 1).
        /// </summary>
        public double SpearmanCorrelation { get; set; }

        /// <summary>
        /// Среднеквадратичная ошибка (Root Mean Square Error, RMSE).
        /// Показывает среднее отклонение предсказанных значений от фактических в тех же единицах измерения.
        /// </summary>
        public double RMSE { get; set; }

        /// <summary>
        /// Средняя абсолютная ошибка (Mean Absolute Error, MAE).
        /// Среднее арифметическое абсолютных разностей между предсказанными и фактическими значениями.
        /// </summary>
        public double MAE { get; set; }

        /// <summary>
        /// Коэффициент детерминации (R-квадрат, R²).
        /// Показывает долю дисперсии зависимой переменной, объясненную моделью (от 0 до 1 для идеальной модели).
        /// </summary>
        public double R2 { get; set; }

        /// <summary>
        /// Массив интерполированных значений.
        /// Используется для визуализации сглаженной линии тренда или дальнейшей математической обработки.
        /// </summary>
        public required double[] InterpolatedBlueValues { get; set; }
    }
}
