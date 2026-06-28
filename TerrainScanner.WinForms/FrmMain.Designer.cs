namespace TerrainScanner.WinForms
{
    partial class FrmMain
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
            PnlTerrain = new Panel();
            BtSelectMap = new Button();
            PnlGraph = new Panel();
            panel1 = new Panel();
            BtnClearLog = new Button();
            BtnDroneSimulate = new Button();
            BtnGraph = new Button();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            label5 = new Label();
            PearsonCorrelation = new Label();
            SpearmanCorrelation = new Label();
            RMSE = new Label();
            MAE = new Label();
            R2 = new Label();
            label6 = new Label();
            InterpolatedBlueValues = new Label();
            label7 = new Label();
            label8 = new Label();
            CellX = new Label();
            CellY = new Label();
            panel2 = new Panel();
            splitContainer2 = new SplitContainer();
            label9 = new Label();
            TxtLog = new RichTextBox();
            splitContainer1 = new SplitContainer();
            panel1.SuspendLayout();
            panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer2).BeginInit();
            splitContainer2.Panel1.SuspendLayout();
            splitContainer2.Panel2.SuspendLayout();
            splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            SuspendLayout();
            // 
            // PnlTerrain
            // 
            PnlTerrain.Dock = DockStyle.Fill;
            PnlTerrain.Location = new Point(0, 0);
            PnlTerrain.Name = "PnlTerrain";
            PnlTerrain.Size = new Size(622, 276);
            PnlTerrain.TabIndex = 0;
            // 
            // BtSelectMap
            // 
            BtSelectMap.Location = new Point(286, 3);
            BtSelectMap.Name = "BtSelectMap";
            BtSelectMap.Size = new Size(95, 46);
            BtSelectMap.TabIndex = 1;
            BtSelectMap.Text = "Выбор карты";
            BtSelectMap.UseVisualStyleBackColor = true;
            BtSelectMap.Click += BtSelectMap_Click;
            // 
            // PnlGraph
            // 
            PnlGraph.Dock = DockStyle.Fill;
            PnlGraph.Location = new Point(0, 0);
            PnlGraph.Name = "PnlGraph";
            PnlGraph.Size = new Size(622, 282);
            PnlGraph.TabIndex = 0;
            // 
            // panel1
            // 
            panel1.BorderStyle = BorderStyle.FixedSingle;
            panel1.Controls.Add(BtnClearLog);
            panel1.Controls.Add(BtnDroneSimulate);
            panel1.Controls.Add(BtnGraph);
            panel1.Controls.Add(BtSelectMap);
            panel1.Dock = DockStyle.Top;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(903, 52);
            panel1.TabIndex = 3;
            // 
            // BtnClearLog
            // 
            BtnClearLog.Location = new Point(387, 3);
            BtnClearLog.Name = "BtnClearLog";
            BtnClearLog.Size = new Size(95, 46);
            BtnClearLog.TabIndex = 5;
            BtnClearLog.Text = "Очистить лог";
            BtnClearLog.UseVisualStyleBackColor = true;
            BtnClearLog.Click += BtnClearLog_Click;
            // 
            // BtnDroneSimulate
            // 
            BtnDroneSimulate.Location = new Point(173, 3);
            BtnDroneSimulate.Name = "BtnDroneSimulate";
            BtnDroneSimulate.Size = new Size(107, 46);
            BtnDroneSimulate.TabIndex = 4;
            BtnDroneSimulate.Text = "Симулировать полет дрона";
            BtnDroneSimulate.UseVisualStyleBackColor = true;
            BtnDroneSimulate.Click += BtnDroneSimulate_Click;
            // 
            // BtnGraph
            // 
            BtnGraph.Location = new Point(3, 3);
            BtnGraph.Name = "BtnGraph";
            BtnGraph.Size = new Size(164, 46);
            BtnGraph.TabIndex = 3;
            BtnGraph.Text = "Симулировать из нарисованного пути";
            BtnGraph.UseVisualStyleBackColor = true;
            BtnGraph.Click += BtnGraph_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(5, 3);
            label1.Name = "label1";
            label1.Size = new Size(206, 15);
            label1.TabIndex = 5;
            label1.Text = "Коэффициент корреляции Пирсона";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(2, 46);
            label2.Name = "label2";
            label2.Size = new Size(269, 15);
            label2.TabIndex = 6;
            label2.Text = "Коэффициент ранговой корреляции Спирмена";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(2, 88);
            label3.Name = "label3";
            label3.Size = new Size(169, 15);
            label3.TabIndex = 7;
            label3.Text = "Среднеквадратичная ошибка";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(2, 132);
            label4.Name = "label4";
            label4.Size = new Size(170, 15);
            label4.TabIndex = 8;
            label4.Text = "Средняя абсолютная ошибка";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(4, 180);
            label5.Name = "label5";
            label5.Size = new Size(167, 15);
            label5.TabIndex = 9;
            label5.Text = "Коэффициент детерминации";
            // 
            // PearsonCorrelation
            // 
            PearsonCorrelation.AutoSize = true;
            PearsonCorrelation.Location = new Point(7, 18);
            PearsonCorrelation.Name = "PearsonCorrelation";
            PearsonCorrelation.Size = new Size(0, 15);
            PearsonCorrelation.TabIndex = 10;
            // 
            // SpearmanCorrelation
            // 
            SpearmanCorrelation.AutoSize = true;
            SpearmanCorrelation.Location = new Point(5, 61);
            SpearmanCorrelation.Name = "SpearmanCorrelation";
            SpearmanCorrelation.Size = new Size(0, 15);
            SpearmanCorrelation.TabIndex = 11;
            // 
            // RMSE
            // 
            RMSE.AutoSize = true;
            RMSE.Location = new Point(5, 103);
            RMSE.Name = "RMSE";
            RMSE.Size = new Size(0, 15);
            RMSE.TabIndex = 12;
            // 
            // MAE
            // 
            MAE.AutoSize = true;
            MAE.Location = new Point(5, 147);
            MAE.Name = "MAE";
            MAE.Size = new Size(0, 15);
            MAE.TabIndex = 13;
            // 
            // R2
            // 
            R2.AutoSize = true;
            R2.Location = new Point(5, 195);
            R2.Name = "R2";
            R2.Size = new Size(0, 15);
            R2.TabIndex = 14;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(5, 227);
            label6.Name = "label6";
            label6.Size = new Size(221, 15);
            label6.TabIndex = 15;
            label6.Text = "Массив интерполированных значений";
            // 
            // InterpolatedBlueValues
            // 
            InterpolatedBlueValues.AutoSize = true;
            InterpolatedBlueValues.Location = new Point(5, 242);
            InterpolatedBlueValues.Name = "InterpolatedBlueValues";
            InterpolatedBlueValues.Size = new Size(0, 15);
            InterpolatedBlueValues.TabIndex = 16;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(7, 269);
            label7.Name = "label7";
            label7.Size = new Size(92, 15);
            label7.TabIndex = 17;
            label7.Text = "Ширина клетки";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(6, 316);
            label8.Name = "label8";
            label8.Size = new Size(87, 15);
            label8.TabIndex = 18;
            label8.Text = "Высота клетки";
            // 
            // CellX
            // 
            CellX.AutoSize = true;
            CellX.Location = new Point(7, 284);
            CellX.Name = "CellX";
            CellX.Size = new Size(0, 15);
            CellX.TabIndex = 19;
            // 
            // CellY
            // 
            CellY.AutoSize = true;
            CellY.Location = new Point(7, 331);
            CellY.Name = "CellY";
            CellY.Size = new Size(0, 15);
            CellY.TabIndex = 20;
            // 
            // panel2
            // 
            panel2.Controls.Add(splitContainer2);
            panel2.Dock = DockStyle.Left;
            panel2.Location = new Point(0, 52);
            panel2.Name = "panel2";
            panel2.Size = new Size(281, 562);
            panel2.TabIndex = 21;
            // 
            // splitContainer2
            // 
            splitContainer2.Dock = DockStyle.Fill;
            splitContainer2.Location = new Point(0, 0);
            splitContainer2.Name = "splitContainer2";
            splitContainer2.Orientation = Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            splitContainer2.Panel1.Controls.Add(label1);
            splitContainer2.Panel1.Controls.Add(CellY);
            splitContainer2.Panel1.Controls.Add(RMSE);
            splitContainer2.Panel1.Controls.Add(MAE);
            splitContainer2.Panel1.Controls.Add(CellX);
            splitContainer2.Panel1.Controls.Add(SpearmanCorrelation);
            splitContainer2.Panel1.Controls.Add(label2);
            splitContainer2.Panel1.Controls.Add(R2);
            splitContainer2.Panel1.Controls.Add(label8);
            splitContainer2.Panel1.Controls.Add(PearsonCorrelation);
            splitContainer2.Panel1.Controls.Add(label3);
            splitContainer2.Panel1.Controls.Add(label6);
            splitContainer2.Panel1.Controls.Add(label7);
            splitContainer2.Panel1.Controls.Add(label5);
            splitContainer2.Panel1.Controls.Add(label4);
            splitContainer2.Panel1.Controls.Add(InterpolatedBlueValues);
            // 
            // splitContainer2.Panel2
            // 
            splitContainer2.Panel2.Controls.Add(label9);
            splitContainer2.Panel2.Controls.Add(TxtLog);
            splitContainer2.Size = new Size(281, 562);
            splitContainer2.SplitterDistance = 363;
            splitContainer2.TabIndex = 23;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.BorderStyle = BorderStyle.FixedSingle;
            label9.Location = new Point(0, 0);
            label9.Name = "label9";
            label9.Size = new Size(29, 17);
            label9.TabIndex = 22;
            label9.Text = "Лог";
            // 
            // TxtLog
            // 
            TxtLog.Dock = DockStyle.Fill;
            TxtLog.Location = new Point(0, 0);
            TxtLog.Name = "TxtLog";
            TxtLog.ReadOnly = true;
            TxtLog.Size = new Size(281, 195);
            TxtLog.TabIndex = 21;
            TxtLog.Text = "";
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(281, 52);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Orientation = Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(PnlTerrain);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(PnlGraph);
            splitContainer1.Size = new Size(622, 562);
            splitContainer1.SplitterDistance = 276;
            splitContainer1.TabIndex = 22;
            // 
            // FrmMain
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(903, 614);
            Controls.Add(splitContainer1);
            Controls.Add(panel2);
            Controls.Add(panel1);
            Name = "FrmMain";
            Text = "Система \"ГЛАЗА\" (Хакатон 26.06)";
            panel1.ResumeLayout(false);
            panel2.ResumeLayout(false);
            splitContainer2.Panel1.ResumeLayout(false);
            splitContainer2.Panel1.PerformLayout();
            splitContainer2.Panel2.ResumeLayout(false);
            splitContainer2.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer2).EndInit();
            splitContainer2.ResumeLayout(false);
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel PnlTerrain;
        private Button BtSelectMap;
        private Panel PnlGraph;
        private Panel panel1;
        private Button BtnGraph;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private Label label5;
        private Label PearsonCorrelation;
        private Label SpearmanCorrelation;
        private Label RMSE;
        private Label MAE;
        private Label R2;
        private Label label6;
        private Label InterpolatedBlueValues;
        private Button BtnDroneSimulate;
        private Label label7;
        private Label label8;
        private Label CellX;
        private Label CellY;
        private Panel panel2;
        private SplitContainer splitContainer1;
        private RichTextBox TxtLog;
        private Button BtnClearLog;
        private Label label9;
        private SplitContainer splitContainer2;
    }
}
