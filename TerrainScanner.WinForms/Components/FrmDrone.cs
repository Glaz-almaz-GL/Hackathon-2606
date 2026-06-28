using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace TerrainScanner.WinForms.Components
{
    public partial class FrmDrone : Form
    {
        /// <summary>
        /// Конструктор. Принимает размеры карты для корректной валидации.
        /// </summary>
        public FrmDrone(int mapRows, int mapColumns)
        {
            InitializeComponent();
            AcceptButton = BtnOk;
            CancelButton = BtnCancel;

            _mapRows = mapRows;
            _mapColumns = mapColumns;

            // Подсказки для пользователя
            TxtRow.Text = "0";
            TxtColumn.Text = "0";
            LblRowBounds.Text = $"Координаты клетки (строка) [0, {mapRows - 1}]";
            LblColumnBounds.Text = $"Координаты клетки (столбец) [0, {mapColumns - 1}]";
        }

        private readonly int _mapRows;
        private readonly int _mapColumns;

        /// <summary>
        /// Валидация данных и закрытие диалога
        /// </summary>
        private void ValidateAndClose()
        {
            // 1. Частота измерений (Гц) — должна быть положительной
            if (!double.TryParse(TxtFrequency.Text, out double frequency) || frequency <= 0)
            {
                ShowValidationError("Частота должна быть положительным числом");
                return;
            }

            // 2. Высота дрона (метры) — должна быть положительной
            if (!double.TryParse(TxtHeight.Text, out double droneHeight) || droneHeight <= 0)
            {
                ShowValidationError("Высота дрона должна быть положительным числом");
                return;
            }

            // 3. Уровень шума (метры) — неотрицательный
            if (!double.TryParse(TxtNoise.Text, out double noise) || noise < 0)
            {
                ShowValidationError("Уровень шума не может быть отрицательным");
                return;
            }

            // 4. Row — индекс строки в пределах карты
            if (!int.TryParse(TxtRow.Text, out int row) || row < 0 || row >= _mapRows)
            {
                ShowValidationError($"Row должен быть в диапазоне [0, {_mapRows - 1}]");
                return;
            }

            // 5. Column — индекс колонки в пределах карты
            if (!int.TryParse(TxtColumn.Text, out int column) || column < 0 || column >= _mapColumns)
            {
                ShowValidationError($"Column должен быть в диапазоне [0, {_mapColumns - 1}]");
                return;
            }

            // 6. Курс: диапазон [0, 360)
            if (!double.TryParse(TxtHeading.Text, out double heading) || heading < 0 || heading >= 360)
            {
                ShowValidationError("Курс должен быть в диапазоне [0, 360)");
                return;
            }

            // 7. Скорость (м/с) — положительная
            if (!double.TryParse(TxtSpeed.Text, out double speed) || speed <= 0)
            {
                ShowValidationError("Скорость должна быть положительным числом");
                return;
            }

            // 8. Длительность (сек) — положительная
            if (!double.TryParse(TxtTime.Text, out double duration) || duration <= 0)
            {
                ShowValidationError("Длительность должна быть положительным числом");
                return;
            }

            // Если все данные валидны, устанавливаем свойства
            Frequency = frequency;
            DroneHeight = droneHeight;
            Noise = noise;
            Row = row;
            Column = column;
            Heading = heading;
            Speed = speed;
            Duration = duration;

            DialogResult = DialogResult.OK;
        }

        private void ShowValidationError(string message)
        {
            MessageBox.Show(message, "Ошибка валидации", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            DialogResult = DialogResult.None; // Не закрываем диалог
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            ValidateAndClose();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        #region Properties for returning data

        /// <summary>
        /// Частота измерений радиовысотомера (Гц)
        /// </summary>
        public double Frequency { get; private set; }

        /// <summary>
        /// Высота полета дрона над уровнем моря (метры)
        /// </summary>
        public double DroneHeight { get; private set; }

        /// <summary>
        /// Амплитуда шума измерений (метры, ±)
        /// </summary>
        public double Noise { get; private set; }

        /// <summary>
        /// Индекс строки стартовой позиции дрона в сетке карты
        /// </summary>
        public int Row { get; private set; }

        /// <summary>
        /// Индекс колонки стартовой позиции дрона в сетке карты
        /// </summary>
        public int Column { get; private set; }

        /// <summary>
        /// Курс дрона (градусы, диапазон [0, 360))
        /// </summary>
        public double Heading { get; private set; }

        /// <summary>
        /// Скорость дрона (м/с)
        /// </summary>
        public double Speed { get; private set; }

        /// <summary>
        /// Длительность симуляции (секунды)
        /// </summary>
        public double Duration { get; private set; }

        #endregion
    }
}