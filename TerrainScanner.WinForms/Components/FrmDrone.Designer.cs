namespace TerrainScanner.WinForms.Components
{
    partial class FrmDrone
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            TxtNoise = new TextBox();
            TxtSpeed = new TextBox();
            TxtTime = new TextBox();
            TxtHeight = new TextBox();
            TxtHeading = new TextBox();
            TxtRow = new TextBox();
            TxtFrequency = new TextBox();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            label5 = new Label();
            LblRowBounds = new Label();
            label7 = new Label();
            BtnOk = new Button();
            BtnCancel = new Button();
            LblColumnBounds = new Label();
            TxtColumn = new TextBox();
            SuspendLayout();
            // 
            // TxtNoise
            // 
            TxtNoise.Location = new Point(12, 27);
            TxtNoise.Name = "TxtNoise";
            TxtNoise.Size = new Size(166, 23);
            TxtNoise.TabIndex = 0;
            TxtNoise.Text = "30.0";
            // 
            // TxtSpeed
            // 
            TxtSpeed.Location = new Point(12, 87);
            TxtSpeed.Name = "TxtSpeed";
            TxtSpeed.Size = new Size(166, 23);
            TxtSpeed.TabIndex = 1;
            TxtSpeed.Text = "50";
            // 
            // TxtTime
            // 
            TxtTime.Location = new Point(12, 138);
            TxtTime.Name = "TxtTime";
            TxtTime.Size = new Size(166, 23);
            TxtTime.TabIndex = 2;
            TxtTime.Text = "60";
            // 
            // TxtHeight
            // 
            TxtHeight.Location = new Point(12, 190);
            TxtHeight.Name = "TxtHeight";
            TxtHeight.Size = new Size(166, 23);
            TxtHeight.TabIndex = 3;
            TxtHeight.Text = "2500";
            // 
            // TxtHeading
            // 
            TxtHeading.Location = new Point(12, 239);
            TxtHeading.Name = "TxtHeading";
            TxtHeading.Size = new Size(166, 23);
            TxtHeading.TabIndex = 4;
            TxtHeading.Text = "0.0";
            // 
            // TxtRow
            // 
            TxtRow.Location = new Point(12, 287);
            TxtRow.Name = "TxtRow";
            TxtRow.Size = new Size(166, 23);
            TxtRow.TabIndex = 5;
            TxtRow.Text = "0";
            // 
            // TxtFrequency
            // 
            TxtFrequency.Location = new Point(12, 389);
            TxtFrequency.Name = "TxtFrequency";
            TxtFrequency.Size = new Size(166, 23);
            TxtFrequency.TabIndex = 6;
            TxtFrequency.Text = "1.0";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(157, 15);
            label1.TabIndex = 7;
            label1.Text = "Шум радиовысотомера (м)";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 69);
            label2.Name = "label2";
            label2.Size = new Size(126, 15);
            label2.TabIndex = 8;
            label2.Text = "Скорость дрона (м/с)";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(12, 120);
            label3.Name = "label3";
            label3.Size = new Size(100, 15);
            label3.TabIndex = 9;
            label3.Text = "Время полета (с)";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(12, 172);
            label4.Name = "label4";
            label4.Size = new Size(108, 15);
            label4.TabIndex = 10;
            label4.Text = "Высота полета (м)";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(12, 221);
            label5.Name = "label5";
            label5.Size = new Size(47, 15);
            label5.TabIndex = 11;
            label5.Text = "Азимут";
            // 
            // LblRowBounds
            // 
            LblRowBounds.AutoSize = true;
            LblRowBounds.Location = new Point(12, 269);
            LblRowBounds.Name = "LblRowBounds";
            LblRowBounds.Size = new Size(166, 15);
            LblRowBounds.TabIndex = 12;
            LblRowBounds.Text = "Стартовая позиция (Широта)";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(12, 371);
            label7.Name = "label7";
            label7.Size = new Size(180, 15);
            label7.TabIndex = 13;
            label7.Text = "Частота получения данных (Гц)";
            // 
            // BtnOk
            // 
            BtnOk.Location = new Point(103, 475);
            BtnOk.Name = "BtnOk";
            BtnOk.Size = new Size(75, 23);
            BtnOk.TabIndex = 14;
            BtnOk.Text = "OK";
            BtnOk.UseVisualStyleBackColor = true;
            BtnOk.Click += BtnOk_Click;
            // 
            // BtnCancel
            // 
            BtnCancel.Location = new Point(10, 475);
            BtnCancel.Name = "BtnCancel";
            BtnCancel.Size = new Size(75, 23);
            BtnCancel.TabIndex = 15;
            BtnCancel.Text = "Отмена";
            BtnCancel.UseVisualStyleBackColor = true;
            BtnCancel.Click += BtnCancel_Click;
            // 
            // LblColumnBounds
            // 
            LblColumnBounds.AutoSize = true;
            LblColumnBounds.Location = new Point(12, 322);
            LblColumnBounds.Name = "LblColumnBounds";
            LblColumnBounds.Size = new Size(168, 15);
            LblColumnBounds.TabIndex = 17;
            LblColumnBounds.Text = "Стартовая позиция (Долгота)";
            // 
            // TxtColumn
            // 
            TxtColumn.Location = new Point(12, 340);
            TxtColumn.Name = "TxtColumn";
            TxtColumn.Size = new Size(166, 23);
            TxtColumn.TabIndex = 16;
            TxtColumn.Text = "0";
            // 
            // FrmDrone
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(197, 527);
            Controls.Add(LblColumnBounds);
            Controls.Add(TxtColumn);
            Controls.Add(BtnCancel);
            Controls.Add(BtnOk);
            Controls.Add(label7);
            Controls.Add(LblRowBounds);
            Controls.Add(label5);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(TxtFrequency);
            Controls.Add(TxtRow);
            Controls.Add(TxtHeading);
            Controls.Add(TxtHeight);
            Controls.Add(TxtTime);
            Controls.Add(TxtSpeed);
            Controls.Add(TxtNoise);
            FormBorderStyle = FormBorderStyle.SizableToolWindow;
            Name = "FrmDrone";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Параметры полета дрона";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox TxtNoise;
        private TextBox TxtSpeed;
        private TextBox TxtTime;
        private TextBox TxtHeight;
        private TextBox TxtHeading;
        private TextBox TxtRow;
        private TextBox TxtFrequency;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private Label label5;
        private Label LblRowBounds;
        private Label label7;
        private Button BtnOk;
        private Button BtnCancel;
        private Label LblColumnBounds;
        private TextBox TxtColumn;
    }
}