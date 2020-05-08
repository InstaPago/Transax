namespace InstaTransfer.Scraper
{
    partial class SDateSelection
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SDateSelection));
            this.lDateStart = new System.Windows.Forms.Label();
            this.lDateEnd = new System.Windows.Forms.Label();
            this.dtpDateStart = new System.Windows.Forms.DateTimePicker();
            this.dtpDateEnd = new System.Windows.Forms.DateTimePicker();
            this.label1 = new System.Windows.Forms.Label();
            this.bDateRangeCancel = new System.Windows.Forms.Button();
            this.bDateRangeAccept = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lDateStart
            // 
            this.lDateStart.AutoSize = true;
            this.lDateStart.Location = new System.Drawing.Point(9, 41);
            this.lDateStart.Name = "lDateStart";
            this.lDateStart.Size = new System.Drawing.Size(41, 13);
            this.lDateStart.TabIndex = 0;
            this.lDateStart.Text = "Desde:";
            // 
            // lDateEnd
            // 
            this.lDateEnd.AutoSize = true;
            this.lDateEnd.Location = new System.Drawing.Point(9, 88);
            this.lDateEnd.Name = "lDateEnd";
            this.lDateEnd.Size = new System.Drawing.Size(38, 13);
            this.lDateEnd.TabIndex = 1;
            this.lDateEnd.Text = "Hasta:";
            // 
            // dtpDateStart
            // 
            this.dtpDateStart.CustomFormat = "dd/mm/yyyy";
            this.dtpDateStart.Location = new System.Drawing.Point(59, 41);
            this.dtpDateStart.Name = "dtpDateStart";
            this.dtpDateStart.Size = new System.Drawing.Size(200, 20);
            this.dtpDateStart.TabIndex = 2;
            // 
            // dtpDateEnd
            // 
            this.dtpDateEnd.CustomFormat = "dd/mm/yyyy";
            this.dtpDateEnd.Location = new System.Drawing.Point(59, 88);
            this.dtpDateEnd.Name = "dtpDateEnd";
            this.dtpDateEnd.Size = new System.Drawing.Size(200, 20);
            this.dtpDateEnd.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(95, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Rango de Fechas:";
            // 
            // bDateRangeCancel
            // 
            this.bDateRangeCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.bDateRangeCancel.Location = new System.Drawing.Point(197, 128);
            this.bDateRangeCancel.Name = "bDateRangeCancel";
            this.bDateRangeCancel.Size = new System.Drawing.Size(75, 23);
            this.bDateRangeCancel.TabIndex = 5;
            this.bDateRangeCancel.Text = "Cancelar";
            this.bDateRangeCancel.UseVisualStyleBackColor = true;
            // 
            // bDateRangeAccept
            // 
            this.bDateRangeAccept.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.bDateRangeAccept.Location = new System.Drawing.Point(116, 128);
            this.bDateRangeAccept.Name = "bDateRangeAccept";
            this.bDateRangeAccept.Size = new System.Drawing.Size(75, 23);
            this.bDateRangeAccept.TabIndex = 6;
            this.bDateRangeAccept.Text = "Aceptar";
            this.bDateRangeAccept.UseVisualStyleBackColor = true;
            this.bDateRangeAccept.Click += new System.EventHandler(this.bDateRangeAccept_Click);
            // 
            // SDateSelection
            // 
            this.AcceptButton = this.bDateRangeAccept;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.bDateRangeCancel;
            this.ClientSize = new System.Drawing.Size(284, 163);
            this.Controls.Add(this.bDateRangeAccept);
            this.Controls.Add(this.bDateRangeCancel);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.dtpDateEnd);
            this.Controls.Add(this.dtpDateStart);
            this.Controls.Add(this.lDateEnd);
            this.Controls.Add(this.lDateStart);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "SDateSelection";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Exportar Movimientos";
            this.TopMost = true;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lDateStart;
        private System.Windows.Forms.Label lDateEnd;
        private System.Windows.Forms.DateTimePicker dtpDateStart;
        private System.Windows.Forms.DateTimePicker dtpDateEnd;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button bDateRangeCancel;
        private System.Windows.Forms.Button bDateRangeAccept;
    }
}