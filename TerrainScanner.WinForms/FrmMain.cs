using System.Numerics;
using TerrainScanner.Core;
using TerrainScanner.Core.Generators;
using TerrainScanner.Core.Models;
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
            using var openFileDialog = new OpenFileDialog
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
                MessageBox.Show($"Не удалось загрузить карту:\n{ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #region NMEA
        private void GenerateNmeaData()
        {
            LogService.Log("=== Запуск генерации NMEA по пути пользователя ===");

            var path = _drawerControl.GetUserPathAs2D();

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

            foreach (var point in path)
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
            var correlationResult = TerrainCorrelation.CalculateCorrelationDetailed(path, generatedPath);

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
            var previewValues = correlationResult.InterpolatedBlueValues.Take(50);
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

        private void GenerateNmeaDataWithDrone()
        {
            if (_map is null)
            {
                LogService.Log("ОШИБКА: Карта не загружена, симуляция невозможна");
                MessageBox.Show("Сначала загрузите карту!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Передаём размеры карты в диалог для валидации
            using FrmDrone frmDroneParams = new(_map.Rows, _map.Columns);

            // 1. Получаем параметры от пользователя
            if (frmDroneParams.ShowDialog(this) != DialogResult.OK)
            {
                LogService.Log("Пользователь отменил ввод параметров дрона");
                return;
            }

            // 2. Преобразуем индексы ячейки в географические координаты
            double startLat = _map.GetLatitude(frmDroneParams.Row);
            double startLon = _map.GetLongitude(frmDroneParams.Column);

            LogService.Log(
                $"Стартовая ячейка [{frmDroneParams.Row}, {frmDroneParams.Column}] -> " +
                $"координаты ({startLat:F6}, {startLon:F6})");

            // 3. Создаем дрона
            var drone = new Drone
            {
                Altitude = frmDroneParams.DroneHeight,
                Heading = frmDroneParams.Heading,
                Speed = frmDroneParams.Speed,
                Location = new TerrainPoint
                {
                    Latitude = startLat,
                    Longitude = startLon,
                    Height = (float)frmDroneParams.DroneHeight
                }
            };

            LogService.Log(
                $"=== Запуск симуляции полёта дрона ===\n" +
                $"  H={frmDroneParams.DroneHeight}м, Heading={frmDroneParams.Heading}°, " +
                $"Speed={frmDroneParams.Speed}м/с\n" +
                $"  StartCell=[{frmDroneParams.Row}, {frmDroneParams.Column}], " +
                $"Duration={frmDroneParams.Duration}с\n" +
                $"  Freq={frmDroneParams.Frequency}Гц, Noise={frmDroneParams.Noise}м");

            // 4. Симулируем полет
            double updateIntervalSeconds = frmDroneParams.Frequency / 10;
            var simulator = new DroneFlightSimulator(
                drone,
                _map,
                simulationDurationSeconds: frmDroneParams.Duration,
                updateIntervalSeconds: updateIntervalSeconds);

            var droneTrack = simulator.SimulateFlight();

            if (droneTrack.Count == 0)
            {
                LogService.Log("ОШИБКА: Симуляция не вернула ни одной точки");
                MessageBox.Show(
                    "Симуляция не вернула ни одной точки.\n\n" +
                    "Возможные причины:\n" +
                    "• Дрон вылетел за границы карты\n" +
                    "• Начальная ячейка содержит NoData (NaN)\n" +
                    "• Скорость/длительность слишком малы",
                    "Ошибка симуляции",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            LogService.Log($"Трек дрона построен. Точек: {droneTrack.Count}");

            // 5. Отображаем путь дрона на карте
            _drawerControl.SetDronePath(droneTrack);

            // 6. Преобразуем трек в формат (дистанция, высота) для корреляции
            var truePath = droneTrack
                .Select(p => new Vector2(p.Distance, p.TerrainHeight))
                .ToList();

            // 7. Генерируем NMEA-данные и восстанавливаем высоту рельефа
            List<Vector2> measuredPath = [];
            List<string> generatedStrings = [];

            foreach (var point in truePath)
            {
                string nmeaResult = _nmeaGenerator.Generate(
                    frmDroneParams.DroneHeight,
                    point.Y,
                    0,
                    frmDroneParams.Noise);

                var parsedRadarAlt = NmeaGenerator.ParseRadarAltitude(nmeaResult);

                if (!parsedRadarAlt.HasValue)
                {
                    LogService.Log($"Предупреждение: Parsed Height from radar is null at X={point.X:F2}");
                    continue;
                }

                generatedStrings.Add(nmeaResult);

                // Восстанавливаем высоту рельефа: Terrain = Drone_Alt - Radar_Alt
                double restoredTerrainHeight = frmDroneParams.DroneHeight - parsedRadarAlt.Value;
                measuredPath.Add(new Vector2(point.X, (float)restoredTerrainHeight));
            }

            if (measuredPath.Count == 0)
            {
                LogService.Log("ОШИБКА: Не удалось восстановить ни одной точки из NMEA");
                return;
            }

            LogService.Log($"Восстановлено измерений: {measuredPath.Count}");

            // 8. Расчет корреляции
            var correlationResult = TerrainCorrelation.CalculateCorrelationDetailed(truePath, measuredPath);

            if (correlationResult is null)
            {
                LogService.Log("ОШИБКА: Расчет корреляции вернул null");
                return;
            }

            // 9. Вывод метрик в UI
            PearsonCorrelation.Text = correlationResult.PearsonCorrelation.ToString("F4");
            SpearmanCorrelation.Text = correlationResult.SpearmanCorrelation.ToString("F4");
            RMSE.Text = correlationResult.RMSE.ToString("F2");
            MAE.Text = correlationResult.MAE.ToString("F2");
            R2.Text = correlationResult.R2.ToString("F4");

            var previewValues = correlationResult.InterpolatedBlueValues.Take(50);
            InterpolatedBlueValues.Text = string.Join("; ", previewValues.Select(v => v.ToString("F2")));
            if (correlationResult.InterpolatedBlueValues.Length > 50)
            {
                InterpolatedBlueValues.Text += " ...";
            }

            // 10. Строим графики профиля рельефа
            _graphControl.DrawGraphs(truePath, measuredPath);

            LogService.Log(
                $"=== Симуляция завершена ===\n" +
                $"  Точек трека: {droneTrack.Count}\n" +
                $"  Точек измерений: {measuredPath.Count}\n" +
                $"  Pearson: {correlationResult.PearsonCorrelation:F4}\n" +
                $"  RMSE: {correlationResult.RMSE:F2}\n" +
                $"  R²: {correlationResult.R2:F4}");
        }

        #endregion

        private void BtnGraph_Click(object sender, EventArgs e)
        {
            LogService.Log("Нажата кнопка: Генерация NMEA по пути пользователя");
            GenerateNmeaData();
        }

        private void BtnDroneSimulate_Click(object sender, EventArgs e)
        {
            LogService.Log("Нажата кнопка: Симуляция полёта дрона");
            GenerateNmeaDataWithDrone();
        }

        private void BtnClearLog_Click(object sender, EventArgs e)
        {
            TxtLog.Clear();
            LogService.Clear();
        }
    }
}