using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using TerrainScanner.Core.Services;

namespace TerrainScanner.WinForms.Components
{
    public partial class FrmDrone : Form
    {
        /// <summary>
        /// Конструктор. Принимает размеры карты и географические границы для корректной валидации.
        /// </summary>
        public FrmDrone(int mapRows, int mapColumns, double minLat, double maxLat, double minLon, double maxLon)
        {
            InitializeComponent();
            AcceptButton = BtnOk;
            CancelButton = BtnCancel;

            _mapRows = mapRows;
            _mapColumns = mapColumns;
            _minLat = minLat;
            _maxLat = maxLat;
            _minLon = minLon;
            _maxLon = maxLon;

            // Подсказки для пользователя
            TxtRow.Text = "0";
            TxtColumn.Text = "0";
            LblRowBounds.Text = $"Строка [0, {mapRows - 1}]";
            LblColumnBounds.Text = $"Столбец [0, {mapColumns - 1}]";

            TxtLat.Text = $"{(minLat + maxLat) / 2:F6}";
            TxtLon.Text = $"{(minLon + maxLon) / 2:F6}";
            LblLatBounds.Text = $"Широта [{minLat:F4}, {maxLat:F4}]";
            LblLonBounds.Text = $"Долгота [{minLon:F4}, {maxLon:F4}]";

            // По умолчанию включён режим клеток
            IsGeoMode.Checked = true;
            UpdateCoordinateFieldsVisibility();

            // Подписка на переключение режимов
            IsGeoMode.CheckedChanged += (s, e) => UpdateCoordinateFieldsVisibility();
            ChkUseExternalNmea.CheckedChanged += (s, e) => UpdateNmeaFieldsVisibility();

            // Кнопка выбора NMEA-файла
            BtnSelectNmea.Click += BtnSelectNmea_Click;
            IsGeoMode.CheckedChanged += ChkUseExternalNmea_CheckedChanged;

            // Изначально внешний NMEA выключен
            IsGeoMode.Checked = false;
            UpdateNmeaFieldsVisibility();
        }

        private readonly int _mapRows;
        private readonly int _mapColumns;
        private readonly double _minLat;
        private readonly double _maxLat;
        private readonly double _minLon;
        private readonly double _maxLon;

        /// <summary>
        /// Обновление видимости полей координат в зависимости от выбранного режима
        /// </summary>
        private void UpdateCoordinateFieldsVisibility()
        {
            bool isCellMode = IsGeoMode.Checked;

            // Режим "Строка/Столбец"
            TxtRow.Visible = isCellMode;
            TxtColumn.Visible = isCellMode;
            LblRowBounds.Visible = isCellMode;
            LblColumnBounds.Visible = isCellMode;

            // Режим "Широта/Долгота"
            TxtLat.Visible = !isCellMode;
            TxtLon.Visible = !isCellMode;
            LblLatBounds.Visible = !isCellMode;
            LblLonBounds.Visible = !isCellMode;
        }

        /// <summary>
        /// Обновление видимости полей, связанных с внешним NMEA-файлом
        /// </summary>
        private void UpdateNmeaFieldsVisibility()
        {
            bool useExternal = ChkUseExternalNmea.Checked;
            TxtNmeaPath.Visible = useExternal;
            BtnSelectNmea.Visible = useExternal;
            LblNmeaPath.Visible = useExternal;

            // Если внешний NMEA не используется — доступны параметры шума
            TxtNoise.Enabled = !useExternal;
            LblNoise.Enabled = !useExternal;
        }

        private void BtnSelectNmea_Click(object? sender, EventArgs e)
        {
            using var openFileDialog = new OpenFileDialog
            {
                Filter = "NMEA files (*.nmea;*.txt)|*.nmea;*.txt|All files (*.*)|*.*",
                Title = "Выберите файл с данными высотомера"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                TxtNmeaPath.Text = openFileDialog.FileName;
            }
        }

        private void ChkUseExternalNmea_CheckedChanged(object? sender, EventArgs e)
        {
            UpdateNmeaFieldsVisibility();
        }

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

            // 3. Уровень шума (метры) — неотрицательный (только если не используется внешний NMEA)
            double noise = 0;
            if (!IsGeoMode.Checked)
            {
                if (!double.TryParse(TxtNoise.Text, out noise) || noise < 0)
                {
                    ShowValidationError("Уровень шума не может быть отрицательным");
                    return;
                }
            }

            // 4. Валидация координат в зависимости от режима
            int row = 0, column = 0;
            double lat = 0, lon = 0;

            if (IsGeoMode.Checked)
            {
                if (!int.TryParse(TxtRow.Text, out row) || row < 0 || row >= _mapRows)
                {
                    ShowValidationError($"Строка должна быть в диапазоне [0, {_mapRows - 1}]");
                    return;
                }

                if (!int.TryParse(TxtColumn.Text, out column) || column < 0 || column >= _mapColumns)
                {
                    ShowValidationError($"Столбец должен быть в диапазоне [0, {_mapColumns - 1}]");
                    return;
                }

                LogService.Log($"Режим координат: ячейка [{row}, {column}]");
            }
            else
            {
                if (!double.TryParse(TxtLat.Text, out lat) || lat < _minLat || lat > _maxLat)
                {
                    ShowValidationError($"Широта должна быть в диапазоне [{_minLat:F4}, {_maxLat:F4}]");
                    return;
                }

                if (!double.TryParse(TxtLon.Text, out lon) || lon < _minLon || lon > _maxLon)
                {
                    ShowValidationError($"Долгота должна быть в диапазоне [{_minLon:F4}, {_maxLon:F4}]");
                    return;
                }

                LogService.Log($"Режим координат: гео ({lat:F6}, {lon:F6})");
            }

            // 5. Курс: диапазон [0, 360)
            if (!double.TryParse(TxtHeading.Text, out double heading) || heading < 0 || heading >= 360)
            {
                ShowValidationError("Курс должен быть в диапазоне [0, 360)");
                return;
            }

            // 6. Скорость (м/с) — положительная
            if (!double.TryParse(TxtSpeed.Text, out double speed) || speed <= 0)
            {
                ShowValidationError("Скорость должна быть положительным числом");
                return;
            }

            // 7. Длительность (сек) — положительная
            if (!double.TryParse(TxtTime.Text, out double duration) || duration <= 0)
            {
                ShowValidationError("Длительность должна быть положительным числом");
                return;
            }

            // 8. Валидация внешнего NMEA-файла (если указан)
            string? nmeaFilePath = null;
            if (IsGeoMode.Checked)
            {
                nmeaFilePath = TxtNmeaPath.Text.Trim();
                if (string.IsNullOrWhiteSpace(nmeaFilePath) || !File.Exists(nmeaFilePath))
                {
                    ShowValidationError("Указанный файл NMEA не существует");
                    return;
                }
            }

            // Если все данные валидны, устанавливаем свойства
            Frequency = frequency;
            DroneHeight = droneHeight;
            Noise = noise;
            UseCellCoordinates = IsGeoMode.Checked;
            Row = row;
            Column = column;
            Latitude = lat;
            Longitude = lon;
            Heading = heading;
            Speed = speed;
            Duration = duration;
            NmeaFilePath = nmeaFilePath;

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
        /// Амплитуда шума измерений (метры, ±). Используется только если NmeaFilePath == null.
        /// </summary>
        public double Noise { get; private set; }

        /// <summary>
        /// Флаг: используется ли режим координат "строка/столбец" (true) или "широта/долгота" (false)
        /// </summary>
        public bool UseCellCoordinates { get; private set; }

        /// <summary>
        /// Индекс строки стартовой позиции дрона (используется, если UseCellCoordinates == true)
        /// </summary>
        public int Row { get; private set; }

        /// <summary>
        /// Индекс колонки стартовой позиции дрона (используется, если UseCellCoordinates == true)
        /// </summary>
        public int Column { get; private set; }

        /// <summary>
        /// Широта стартовой позиции дрона (используется, если UseCellCoordinates == false)
        /// </summary>
        public double Latitude { get; private set; }

        /// <summary>
        /// Долгота стартовой позиции дрона (используется, если UseCellCoordinates == false)
        /// </summary>
        public double Longitude { get; private set; }

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

        /// <summary>
        /// Путь к внешнему NMEA-файлу с данными высотомера.
        /// Если null — данные генерируются симулятором.
        /// </summary>
        public string? NmeaFilePath { get; private set; }

        #endregion
    }
}