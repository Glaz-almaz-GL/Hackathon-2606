using System.Numerics;
using TerrainScanner.Core;
using TerrainScanner.Core.Generators;
using TerrainScanner.Core.Models;
using TerrainScanner.Core.Models.Models;
using TerrainScanner.Core.Models.Terrain;
using TerrainScanner.Core.Services;
using TerrainScanner.Core.Simulators;
using TerrainScanner.WinForms.Components;

namespace TerrainScanner.WinForms
{
    public partial class FrmMain : Form
    {
        private readonly DrawerControl _drawerControl;
        private readonly GraphControl _graphControl;

        private TerrainMap? _map;
        private readonly MapLoader _mapLoader;
        private readonly NmeaGenerator _nmeaGenerator;

        public FrmMain()
        {
            InitializeComponent();
            _drawerControl = new()
            {
                Dock = DockStyle.Fill
            };

            _graphControl = new()
            {
                Dock = DockStyle.Fill
            };

            _nmeaGenerator = new();

            _mapLoader = new();

            PnlTerrain.Controls.Clear();
            PnlTerrain.Controls.Add(_drawerControl);
            PnlGraph.Controls.Clear();
            PnlGraph.Controls.Add(_graphControl);
            LogService.OnLog += LogService_OnLog;

            LogService.Log("Приложение запущено");
        }

        /// <summary>
        /// Обработчик события логирования. Потокобезопасно обновляет UI.
        /// </summary>
        private void LogService_OnLog(string message)
        {
            // Потокобезопасный вызов UI-элемента
            if (InvokeRequired)
            {
                Invoke(new Action(() => LogService_OnLog(message)));
                return;
            }

            TxtLog.AppendText(message + Environment.NewLine);
        }

        private void BtSelectMap_Click(object sender, EventArgs e)
        {
            using OpenFileDialog openFileDialog = new()
            {
                Filter = "GeoTiff files (*.tif)|*.tif|All files (*.*)|*.*",
                Title = "Load Map"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                LogService.Log($"Загрузка карты из файла: {openFileDialog.FileName}");
                LoadMap(openFileDialog.FileName);
            }
            else
            {
                LogService.Log("Выбор файла отменён пользователем");
            }
        }

        private void LoadMap(string path)
        {
            try
            {
                _map = null;
                _map = _mapLoader.LoadFromTif(path);

                LogService.Log($"Карта загружена. Размер: {_map.Rows} x {_map.Columns}");

                _drawerControl.SetMap(_map);
                CellX.Text = _map.CellSizeX.ToString("F2") + " м";
                CellY.Text = _map.CellSizeY.ToString("F2") + " м";

                LogService.Log("Карта отображена в DrawerControl");
            }
            catch (Exception ex)
            {
                LogService.Log($"ОШИБКА загрузки карты: {ex.Message}");
                MessageBox.Show($"Не удалось загрузить карту:{Environment.NewLine}{ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #region NMEA
        private void GenerateNmeaData()
        {
            LogService.Log("=== Запуск генерации NMEA по пути пользователя ===");

            List<Vector2> path = _drawerControl.GetUserPathAs2D();

            if (path.Count == 0)
            {
                LogService.Log("Путь пользователя пуст. Нарисуйте путь на карте.");
                MessageBox.Show("Сначала нарисуйте путь на карте!",
                    "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            LogService.Log($"Путь пользователя: {path.Count} точек");

            List<Vector2> generatedPath = [];
            List<string> generatedStrings = [];

            const double droneAltitude = 5000.0;

            foreach (Vector2 point in path)
            {
                // Генерируем NMEA. point.Y - истинная высота рельефа.
                string nmeaResult = _nmeaGenerator.Generate(droneAltitude, point.Y, 0, 30);
                var parsedRadarAlt = NmeaGenerator.ParseRadarAltitude(nmeaResult);

                if (!parsedRadarAlt.HasValue)
                {
                    LogService.Log($"Предупреждение: Parsed Height from radar is null at X={point.X:F2}");
                    continue;
                }

                generatedStrings.Add(nmeaResult);

                double restoredTerrainHeight = droneAltitude - parsedRadarAlt.Value;
                generatedPath.Add(new Vector2(point.X, (float)restoredTerrainHeight));
            }

            LogService.Log($"Сгенерировано NMEA-сообщений: {generatedStrings.Count}");

            // Теперь мы сравниваем "Истинный рельеф" с "Измеренным рельефом (с шумом)"
            CorrelationResult? correlationResult = TerrainCorrelation.CalculateCorrelationDetailed(path, generatedPath);

            if (correlationResult is null)
            {
                LogService.Log("ОШИБКА: Расчет корреляции вернул null");
                return;
            }

            // Вывод метрик в UI
            PearsonCorrelation.Text = correlationResult.PearsonCorrelation.ToString("F4");
            SpearmanCorrelation.Text = correlationResult.SpearmanCorrelation.ToString("F4");
            RMSE.Text = correlationResult.RMSE.ToString("F2");
            MAE.Text = correlationResult.MAE.ToString("F2");
            R2.Text = correlationResult.R2.ToString("F4");

            // Ограничим вывод массива, чтобы не "повесить" UI-контрол
            IEnumerable<double> previewValues = correlationResult.InterpolatedBlueValues.Take(50);
            InterpolatedBlueValues.Text = string.Join("; ", previewValues.Select(v => v.ToString("F2")));
            if (correlationResult.InterpolatedBlueValues.Length > 50)
            {
                InterpolatedBlueValues.Text += " ...";
            }

            // Отрисовка графиков
            _drawerControl.StraightenPath(pointSpacing: 5);
            _graphControl.DrawGraphs(path, generatedPath);

            LogService.Log(
                $"Генерация завершена. Pearson: {correlationResult.PearsonCorrelation:F4}, " +
                $"RMSE: {correlationResult.RMSE:F2}, R²: {correlationResult.R2:F4}");
        }

        #region DroneSimulation
        private void GenerateNmeaDataWithDrone()
        {
            if (_map is null)
            {
                LogService.Log("ОШИБКА: Карта не загружена, симуляция невозможна");
                MessageBox.Show("Сначала загрузите карту!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using FrmDroneParams? frmDroneParams = ShowDroneParametersDialog();
            if (frmDroneParams == null)
            {
                return;
            }

            (double? startLat, double? startLon) = DetermineStartCoordinates(frmDroneParams);

            if (startLat == null || startLon == null)
            {
                return;
            }

            Drone drone = CreateDrone(frmDroneParams, startLat.Value, startLon.Value);
            LogSimulationStart(frmDroneParams, startLat.Value, startLon.Value);

            List<DroneTrackPoint> droneTrack = SimulateFlight(drone, frmDroneParams);
            if (droneTrack == null)
            {
                return;
            }

            _drawerControl.SetDronePath(droneTrack);
            List<Vector2> truePath = BuildTruePath(droneTrack);

            (List<Vector2>? measuredPath, List<string> _) = GenerateMeasuredPath(truePath, frmDroneParams);
            if (measuredPath.Count == 0)
            {
                LogService.Log("ОШИБКА: Не удалось получить ни одной точки измерений");
                return;
            }
            LogService.Log($"Восстановлено измерений: {measuredPath.Count}");

            CorrelationResult? correlationResult = CalculateCorrelation(truePath, measuredPath);
            if (correlationResult == null)
            {
                return;
            }

            UpdateUiWithMetrics(correlationResult, droneTrack, frmDroneParams);
            _graphControl.DrawGraphs(truePath, measuredPath);
            LogSimulationCompletion(frmDroneParams, droneTrack.Count, measuredPath.Count, correlationResult);
        }

        #region Helpers: Инициализация и параметры

        private FrmDroneParams? ShowDroneParametersDialog()
        {
            if (_map == null) { return null; }

            FrmDroneParams frm = new(
                _map.Rows, _map.Columns,
                _map.MinLatitude, _map.MaxLatitude,
                _map.MinLongitude, _map.MaxLongitude);

            if (frm.ShowDialog(this) != DialogResult.OK)
            {
                LogService.Log("Пользователь отменил ввод параметров дрона");
                frm.Dispose();
                return null;
            }
            return frm;
        }

        private (double? startLat, double? startLon) DetermineStartCoordinates(FrmDroneParams p)
        {
            if (_map == null) { return (null, null); }

            if (p.UseCellCoordinates)
            {
                double startLat = _map.GetLatitude(p.Row);
                double startLon = _map.GetLongitude(p.Column);
                LogService.Log($"Стартовая ячейка [{p.Row}, {p.Column}] -> координаты ({startLat:F6}, {startLon:F6})");
                return (startLat, startLon);
            }

            LogService.Log($"Стартовые геокоординаты: ({p.Latitude:F6}, {p.Longitude:F6})");
            return (p.Latitude, p.Longitude);
        }

        private static Drone CreateDrone(FrmDroneParams p, double startLat, double startLon)
        {
            return new Drone
            {
                Altitude = p.DroneHeight,
                Heading = p.Heading,
                Speed = p.Speed,
                Location = new TerrainPoint
                {
                    Latitude = startLat,
                    Longitude = startLon,
                    Height = (float)p.DroneHeight
                }
            };
        }

        private static void LogSimulationStart(FrmDroneParams p, double startLat, double startLon)
        {
            LogService.Log(
                $"=== Запуск симуляции полёта дрона ==={Environment.NewLine}" +
                $"  H={p.DroneHeight}м, Heading={p.Heading}°, Speed={p.Speed}м/с{Environment.NewLine}" +
                $"  Start=({startLat:F6}, {startLon:F6}), Duration={p.Duration}с{Environment.NewLine}" +
                $"  Freq={p.Frequency}Гц");
        }

        #endregion

        #region Helpers: Симуляция и сбор данных

        // Замените DroneTrackPoint на реальный тип элемента, возвращаемого SimulateFlight(), если он называется иначе
        private List<DroneTrackPoint> SimulateFlight(Drone drone, FrmDroneParams p)
        {
            if (_map == null) { return []; }

            double updateIntervalSeconds = 1.0 / p.Frequency;
            DroneFlightSimulator simulator = new(
                drone, _map,
                simulationDurationSeconds: p.Duration,
                updateIntervalSeconds: updateIntervalSeconds);

            List<DroneTrackPoint> droneTrack = simulator.SimulateFlight();

            if (droneTrack.Count == 0)
            {
                LogService.Log("ОШИБКА: Симуляция не вернула ни одной точки");
                MessageBox.Show(
                    $"Симуляция не вернула ни одной точки.{Environment.NewLine}{Environment.NewLine}" +
                    $"Возможные причины:{Environment.NewLine}" +
                    $"• Дрон вылетел за границы карты{Environment.NewLine}" +
                    $"• Начальная ячейка содержит NoData (NaN){Environment.NewLine}" +
                    "• Скорость/длительность слишком малы",
                    "Ошибка симуляции", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return [];
            }

            LogService.Log($"Трек дрона построен. Точек: {droneTrack.Count}");
            return droneTrack;
        }

        private static List<Vector2> BuildTruePath(List<DroneTrackPoint> droneTrack)
        {
            return droneTrack.Select(p => new Vector2(p.Distance, p.TerrainHeight)).ToList();
        }

        private (List<Vector2> measuredPath, List<string> generatedStrings) GenerateMeasuredPath(
            List<Vector2> truePath, FrmDroneParams p)
        {
            List<Vector2> measuredPath = [];
            List<string> generatedStrings = [];

            if (!string.IsNullOrWhiteSpace(p.NmeaFilePath))
            {
                ProcessFileAltitudes(truePath, p, measuredPath, generatedStrings);
            }
            else
            {
                GenerateSimulatedAltitudes(truePath, p, measuredPath, generatedStrings);
            }

            return (measuredPath, generatedStrings);
        }

        private static void ProcessFileAltitudes(
            List<Vector2> truePath, FrmDroneParams p,
            List<Vector2> measuredPath, List<string> generatedStrings)
        {
            LogService.Log($"Чтение данных высотомера из файла: {p.NmeaFilePath}");

            if (string.IsNullOrEmpty(p.NmeaFilePath) || p.NmeaFilePath is null)
            {
                return;
            }

            List<float?> fileAltitudes = NmeaGenerator.ReadRadarAltitudesFromFile(p.NmeaFilePath);
            LogService.Log($"Прочитано значений из файла: {fileAltitudes.Count}");

            int countToProcess = Math.Min(truePath.Count, fileAltitudes.Count);
            if (truePath.Count != fileAltitudes.Count)
            {
                LogService.Log(
                    $"ПРЕДУПРЕЖДЕНИЕ: Количество точек трека ({truePath.Count}) и " +
                    $"значений в файле ({fileAltitudes.Count}) не совпадает. " +
                    $"Будет обработано {countToProcess} точек.");
            }

            for (int i = 0; i < countToProcess; i++)
            {
                Vector2 point = truePath[i];
                var parsedRadarAlt = fileAltitudes[i];

                if (!parsedRadarAlt.HasValue)
                {
                    LogService.Log($"Предупреждение: NMEA-строка #{i} не содержит валидной высоты");
                    continue;
                }

                generatedStrings.Add($"[Файл] #{i}: {parsedRadarAlt.Value:F1}м");
                measuredPath.Add(new Vector2(point.X, (float)(p.DroneHeight - parsedRadarAlt.Value)));
            }
        }

        private void GenerateSimulatedAltitudes(
            List<Vector2> truePath, FrmDroneParams p,
            List<Vector2> measuredPath, List<string> generatedStrings)
        {
            LogService.Log($"Генерация NMEA-данных (шум: ±{p.Noise}м)");

            foreach (Vector2 point in truePath)
            {
                string nmeaResult = _nmeaGenerator.Generate(p.DroneHeight, point.Y, 0, p.Noise);
                var parsedRadarAlt = NmeaGenerator.ParseRadarAltitude(nmeaResult);

                if (!parsedRadarAlt.HasValue)
                {
                    LogService.Log($"Предупреждение: Parsed Height from radar is null at X={point.X:F2}");
                    continue;
                }

                generatedStrings.Add(nmeaResult);
                measuredPath.Add(new Vector2(point.X, (float)(p.DroneHeight - parsedRadarAlt.Value)));
            }
        }

        #endregion

        #region Helpers: Корреляция и UI

        // Замените dynamic на реальный тип результата (например, CorrelationResult)
        private static CorrelationResult? CalculateCorrelation(List<Vector2> truePath, List<Vector2> measuredPath)
        {
            CorrelationResult? correlationResult = TerrainCorrelation.CalculateCorrelationDetailed(truePath, measuredPath);
            if (correlationResult is null)
            {
                LogService.Log("ОШИБКА: Расчет корреляции вернул null");
                return null;
            }
            return correlationResult;
        }

        private void UpdateUiWithMetrics(
            CorrelationResult correlationResult,
            List<DroneTrackPoint> droneTrack,
            FrmDroneParams p)
        {
            PearsonCorrelation.Text = correlationResult.PearsonCorrelation.ToString("F4");
            SpearmanCorrelation.Text = correlationResult.SpearmanCorrelation.ToString("F4");
            RMSE.Text = correlationResult.RMSE.ToString("F2");
            MAE.Text = correlationResult.MAE.ToString("F2");
            R2.Text = correlationResult.R2.ToString("F4");

            DroneTrackPoint lastPoint = droneTrack[^1];
            LblX.Text = lastPoint.Longitude.ToString("F6");
            LblY.Text = lastPoint.Latitude.ToString("F6");
            LblZ.Text = p.DroneHeight.ToString("F6");

            IEnumerable<double> previewValues = correlationResult.InterpolatedBlueValues.Take(50);
            InterpolatedBlueValues.Text = string.Join("; ", previewValues.Select(v => v.ToString("F2")));
            if (correlationResult.InterpolatedBlueValues.Length > 50)
            {
                InterpolatedBlueValues.Text += " ...";
            }
        }

        private static void LogSimulationCompletion(
            FrmDroneParams p, int trackCount, int measurementCount, dynamic correlationResult)
        {
            LogService.Log(
                $"=== Симуляция завершена ==={Environment.NewLine}" +
                $"  Источник данных: {(!string.IsNullOrWhiteSpace(p.NmeaFilePath) ? "Внешний файл" : "Симулятор")}{Environment.NewLine}" +
                $"  Точек трека: {trackCount}{Environment.NewLine}" +
                $"  Точек измерений: {measurementCount}{Environment.NewLine}" +
                $"  Pearson: {correlationResult.PearsonCorrelation:F4}{Environment.NewLine}" +
                $"  RMSE: {correlationResult.RMSE:F2}{Environment.NewLine}" +
                $"  R²: {correlationResult.R2:F4}");
        }

        #endregion
        #endregion

        /// <summary>
        /// Блокирует/разблокирует кнопки управления симуляцией
        /// </summary>
        private void SetSimulationControlsEnabled(bool enabled)
        {
            if (InvokeRequired)
            {
                Invoke(() => SetSimulationControlsEnabled(enabled));
                return;
            }

            BtnDroneSimulate.Enabled = enabled;
            BtnGraph.Enabled = enabled;
            BtSelectMap.Enabled = enabled;
        }

        #endregion

        private void BtnGraph_Click(object sender, EventArgs e)
        {
            GenerateNmeaData();
        }

        private void BtnDroneSimulate_Click(object sender, EventArgs e)
        {
            GenerateNmeaDataWithDrone();
        }

        private void BtnClearLog_Click(object sender, EventArgs e)
        {
            TxtLog.Clear();
            LogService.Clear();
        }
    }
}