using TerrainNavigation.Core.Map;
using TerrainNavigation.Core.Models;
using TerrainNavigation.Core.Navigation;
using TerrainNavigation.Core.Services;
using TerrainNavigation.WinForms.Controls;

namespace TerrainNavigation.WinForms
{
    public partial class MainForm : Form
    {

        private readonly MapLoader _loader;
        private TerrainMap? _map;

        private readonly NavigationCanvasControl _canvas;
        private readonly GraphControl _graphControl;

        public MainForm()
        {
            InitializeComponent();

            _loader = new MapLoader();

            _graphControl = new()
            {
                Dock = DockStyle.Fill
            };

            _canvas = new NavigationCanvasControl(_graphControl)
            {
                Dock = DockStyle.Fill
            };

            mainPanel.Controls.Clear();
            mainPanel.Controls.Add(_canvas);
            graphPanel.Controls.Add(_graphControl);

            LogService.OnLog += LogService_OnLog;
        }

        private void LogService_OnLog(string msg)
        {

            if (InvokeRequired)
            {
                BeginInvoke(new Action(() =>
                {
                    txtLog.AppendText(msg + Environment.NewLine);
                }));
            }
            else
            {
                txtLog.AppendText(msg + Environment.NewLine);
            }
        }

        private void btMapLoad_Click(object sender, EventArgs e)
        {
            //using var openFileDialog = new OpenFileDialog
            //{
            //    Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
            //    Title = "Load Map"
            //};

            using var openFileDialog = new OpenFileDialog
            {
                Filter = "GeoTiff files (*.tif)|*.tif|All files (*.*)|*.*",
                Title = "Load Map"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                LogService.Log("Загрузка карты из файла: " + openFileDialog.FileName);
                LoadMap(openFileDialog.FileName);
            }
        }

        private void GenerateTestData()
        {
            if (_map is null) return;

            var path = _canvas.GetUserPath();
            var firstPoint = path[0];
            var lastPoint = path[^1];
        }

        private void LoadMap(string path)
        {
            _map = null;
            _map = _loader.LoadFromTif(path);

            LogService.Log($"MAP LOADED {_map.Rows} x {_map.Columns}");

            _canvas.SetMap(_map);


            LogService.Log("Карта загружена");
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            _canvas.ForceRunSimulation(_noiseSlider.Value);
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            _canvas.ClearPath();
        }

        private void _noiseSlider_ValueChanged(object sender, EventArgs e)
        {
            _canvas.SetNoise(_noiseSlider.Value);
        }

        private void btGeneration_Click(object sender, EventArgs e)
        {
            _canvas.GeneratePath(pathSlider.Value);
            LogService.Log("Начинаем генерацию пути протяженностью " + pathSlider.Value + " метров");
        }

        private void btTestData_Click(object sender, EventArgs e)
        {
            GenerateTestData();
        }
    }
}
