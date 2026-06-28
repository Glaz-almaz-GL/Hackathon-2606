namespace TerrainNavigation.WinForms
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btMapLoad = new Button();
            panel1 = new Panel();
            label1 = new Label();
            pathSlider = new TrackBar();
            btGeneration = new Button();
            _statsLabel = new Label();
            _noiseSlider = new TrackBar();
            button2 = new Button();
            btnRun = new Button();
            mainPanel = new Panel();
            txtLog = new TextBox();
            splitContainer1 = new SplitContainer();
            graphPanel = new Panel();
            btTestData = new Button();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pathSlider).BeginInit();
            ((System.ComponentModel.ISupportInitialize)_noiseSlider).BeginInit();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            SuspendLayout();
            // 
            // btMapLoad
            // 
            btMapLoad.Location = new Point(4, 3);
            btMapLoad.Name = "btMapLoad";
            btMapLoad.Size = new Size(112, 42);
            btMapLoad.TabIndex = 0;
            btMapLoad.Text = "Загрузка карты";
            btMapLoad.UseVisualStyleBackColor = true;
            btMapLoad.Click += btMapLoad_Click;
            // 
            // panel1
            // 
            panel1.Controls.Add(label1);
            panel1.Controls.Add(pathSlider);
            panel1.Controls.Add(btGeneration);
            panel1.Controls.Add(_statsLabel);
            panel1.Controls.Add(_noiseSlider);
            panel1.Controls.Add(button2);
            panel1.Controls.Add(btnRun);
            panel1.Controls.Add(btMapLoad);
            panel1.Dock = DockStyle.Top;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(1237, 49);
            panel1.TabIndex = 2;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(240, 17);
            label1.Name = "label1";
            label1.Size = new Size(91, 15);
            label1.TabIndex = 7;
            label1.Text = "Длина пути, км";
            // 
            // pathSlider
            // 
            pathSlider.Location = new Point(387, 3);
            pathSlider.Maximum = 100;
            pathSlider.Name = "pathSlider";
            pathSlider.Size = new Size(148, 45);
            pathSlider.TabIndex = 6;
            // 
            // btGeneration
            // 
            btGeneration.Location = new Point(122, 3);
            btGeneration.Name = "btGeneration";
            btGeneration.Size = new Size(112, 42);
            btGeneration.TabIndex = 5;
            btGeneration.Text = "Генерация пути";
            btGeneration.UseVisualStyleBackColor = true;
            btGeneration.Click += btGeneration_Click;
            // 
            // _statsLabel
            // 
            _statsLabel.AutoSize = true;
            _statsLabel.Location = new Point(794, 17);
            _statsLabel.Name = "_statsLabel";
            _statsLabel.Size = new Size(167, 15);
            _statsLabel.TabIndex = 4;
            _statsLabel.Text = "Предел ошибки высотомера";
            // 
            // _noiseSlider
            // 
            _noiseSlider.Location = new Point(967, 1);
            _noiseSlider.Maximum = 100;
            _noiseSlider.Name = "_noiseSlider";
            _noiseSlider.Size = new Size(194, 45);
            _noiseSlider.TabIndex = 3;
            _noiseSlider.ValueChanged += _noiseSlider_ValueChanged;
            // 
            // button2
            // 
            button2.Location = new Point(676, 3);
            button2.Name = "button2";
            button2.Size = new Size(112, 42);
            button2.TabIndex = 2;
            button2.Text = "Очистить";
            button2.UseVisualStyleBackColor = true;
            button2.Click += btnClear_Click;
            // 
            // btnRun
            // 
            btnRun.Location = new Point(558, 3);
            btnRun.Name = "btnRun";
            btnRun.Size = new Size(112, 42);
            btnRun.TabIndex = 1;
            btnRun.Text = "Запуск симуляции";
            btnRun.UseVisualStyleBackColor = true;
            btnRun.Click += btnRun_Click;
            // 
            // mainPanel
            // 
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.Location = new Point(0, 0);
            mainPanel.Name = "mainPanel";
            mainPanel.Size = new Size(637, 315);
            mainPanel.TabIndex = 3;
            // 
            // txtLog
            // 
            txtLog.Location = new Point(655, 51);
            txtLog.Multiline = true;
            txtLog.Name = "txtLog";
            txtLog.Size = new Size(233, 495);
            txtLog.TabIndex = 4;
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Left;
            splitContainer1.Location = new Point(0, 49);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Orientation = Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(mainPanel);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(graphPanel);
            splitContainer1.Size = new Size(637, 557);
            splitContainer1.SplitterDistance = 315;
            splitContainer1.TabIndex = 5;
            // 
            // graphPanel
            // 
            graphPanel.Dock = DockStyle.Fill;
            graphPanel.Location = new Point(0, 0);
            graphPanel.Name = "graphPanel";
            graphPanel.Size = new Size(637, 238);
            graphPanel.TabIndex = 0;
            // 
            // btTestData
            // 
            btTestData.Location = new Point(894, 55);
            btTestData.Name = "btTestData";
            btTestData.Size = new Size(112, 42);
            btTestData.TabIndex = 8;
            btTestData.Text = "Тестовые данные";
            btTestData.UseVisualStyleBackColor = true;
            btTestData.Click += btTestData_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1237, 606);
            Controls.Add(btTestData);
            Controls.Add(splitContainer1);
            Controls.Add(txtLog);
            Controls.Add(panel1);
            Name = "MainForm";
            Text = "Навигация по рельефу. Хакатон 26.06";
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pathSlider).EndInit();
            ((System.ComponentModel.ISupportInitialize)_noiseSlider).EndInit();
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btMapLoad;
        private Panel panel1;
        private Label _statsLabel;
        private TrackBar _noiseSlider;
        private Button button2;
        private Button btnRun;
        private Panel mainPanel;
        private TextBox txtLog;
        private Label label1;
        private TrackBar pathSlider;
        private Button btGeneration;
        private SplitContainer splitContainer1;
        private Panel graphPanel;
        private Button btTestData;
    }
}
