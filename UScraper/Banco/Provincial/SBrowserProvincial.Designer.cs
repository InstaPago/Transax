﻿namespace InstaTransfer.Scraper.Provincial
{
    partial class SBrowserProvincial
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SBrowserProvincial));
            this.wbUmbrellaExplorer = new System.Windows.Forms.WebBrowser();
            this.tsUBrowser = new System.Windows.Forms.ToolStrip();
            this.tsbHome = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.tsddbMenu = new System.Windows.Forms.ToolStripDropDownButton();
            this.tsmiAccount = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiAccountBFI = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiAccountTD = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiDates = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiDay = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiRange = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.tsmiLogout = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbLogin = new System.Windows.Forms.ToolStripButton();
            this.tstbPassword = new System.Windows.Forms.ToolStripTextBox();
            this.tslPassword = new System.Windows.Forms.ToolStripLabel();
            this.tstbUser = new System.Windows.Forms.ToolStripTextBox();
            this.tslUser = new System.Windows.Forms.ToolStripLabel();
            this.tstbRIF = new System.Windows.Forms.ToolStripTextBox();
            this.tslRIF = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.tsslWebPageStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsUBrowser.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // wbUmbrellaExplorer
            // 
            this.wbUmbrellaExplorer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.wbUmbrellaExplorer.Location = new System.Drawing.Point(1, 31);
            this.wbUmbrellaExplorer.MinimumSize = new System.Drawing.Size(20, 20);
            this.wbUmbrellaExplorer.Name = "wbUmbrellaExplorer";
            this.wbUmbrellaExplorer.Size = new System.Drawing.Size(821, 672);
            this.wbUmbrellaExplorer.TabIndex = 0;
            this.wbUmbrellaExplorer.Url = new System.Uri("", System.UriKind.Relative);
            // 
            // tsUBrowser
            // 
            this.tsUBrowser.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbHome,
            this.toolStripSeparator2,
            this.tsddbMenu,
            this.toolStripSeparator3,
            this.tsbLogin,
            this.tstbPassword,
            this.tslPassword,
            this.tstbUser,
            this.tslUser,
            this.tstbRIF,
            this.tslRIF,
            this.toolStripSeparator1});
            this.tsUBrowser.Location = new System.Drawing.Point(0, 0);
            this.tsUBrowser.Name = "tsUBrowser";
            this.tsUBrowser.Padding = new System.Windows.Forms.Padding(0, 5, 1, 0);
            this.tsUBrowser.Size = new System.Drawing.Size(823, 28);
            this.tsUBrowser.TabIndex = 9;
            this.tsUBrowser.Text = "toolStrip1";
            // 
            // tsbHome
            // 
            this.tsbHome.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsbHome.Font = new System.Drawing.Font("FontAwesome", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tsbHome.Image = ((System.Drawing.Image)(resources.GetObject("tsbHome.Image")));
            this.tsbHome.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbHome.Name = "tsbHome";
            this.tsbHome.Size = new System.Drawing.Size(27, 20);
            this.tsbHome.Text = "";
            this.tsbHome.ToolTipText = "Home";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 23);
            // 
            // tsddbMenu
            // 
            this.tsddbMenu.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.tsddbMenu.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsddbMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiAccount,
            this.tsmiDates,
            this.toolStripSeparator4,
            this.tsmiLogout});
            this.tsddbMenu.Font = new System.Drawing.Font("FontAwesome", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tsddbMenu.Image = ((System.Drawing.Image)(resources.GetObject("tsddbMenu.Image")));
            this.tsddbMenu.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsddbMenu.Name = "tsddbMenu";
            this.tsddbMenu.Size = new System.Drawing.Size(28, 20);
            this.tsddbMenu.Text = "";
            this.tsddbMenu.ToolTipText = "Menu";
            // 
            // tsmiAccount
            // 
            this.tsmiAccount.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiAccountBFI,
            this.tsmiAccountTD});
            this.tsmiAccount.Name = "tsmiAccount";
            this.tsmiAccount.Size = new System.Drawing.Size(107, 22);
            this.tsmiAccount.Text = "Cuenta";
            // 
            // tsmiAccountBFI
            // 
            this.tsmiAccountBFI.Checked = true;
            this.tsmiAccountBFI.CheckState = System.Windows.Forms.CheckState.Checked;
            this.tsmiAccountBFI.Name = "tsmiAccountBFI";
            this.tsmiAccountBFI.Size = new System.Drawing.Size(123, 22);
            this.tsmiAccountBFI.Text = "BFI";
            // 
            // tsmiAccountTD
            // 
            this.tsmiAccountTD.Name = "tsmiAccountTD";
            this.tsmiAccountTD.Size = new System.Drawing.Size(123, 22);
            this.tsmiAccountTD.Text = "Transdestino";
            // 
            // tsmiDates
            // 
            this.tsmiDates.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiDay,
            this.tsmiRange});
            this.tsmiDates.Name = "tsmiDates";
            this.tsmiDates.Size = new System.Drawing.Size(107, 22);
            this.tsmiDates.Text = "Fechas";
            // 
            // tsmiDay
            // 
            this.tsmiDay.Checked = true;
            this.tsmiDay.CheckState = System.Windows.Forms.CheckState.Checked;
            this.tsmiDay.Name = "tsmiDay";
            this.tsmiDay.Size = new System.Drawing.Size(106, 22);
            this.tsmiDay.Text = "Dia";
            // 
            // tsmiRange
            // 
            this.tsmiRange.Name = "tsmiRange";
            this.tsmiRange.Size = new System.Drawing.Size(106, 22);
            this.tsmiRange.Text = "Rango...";
            this.tsmiRange.Click += new System.EventHandler(this.tsmiRange_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(104, 6);
            // 
            // tsmiLogout
            // 
            this.tsmiLogout.Font = new System.Drawing.Font("FontAwesome", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tsmiLogout.Name = "tsmiLogout";
            this.tsmiLogout.ShortcutKeyDisplayString = "";
            this.tsmiLogout.Size = new System.Drawing.Size(107, 22);
            this.tsmiLogout.Text = "Salir ";
            this.tsmiLogout.ToolTipText = "Logout";
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 23);
            // 
            // tsbLogin
            // 
            this.tsbLogin.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.tsbLogin.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsbLogin.Font = new System.Drawing.Font("FontAwesome", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tsbLogin.Image = ((System.Drawing.Image)(resources.GetObject("tsbLogin.Image")));
            this.tsbLogin.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbLogin.Name = "tsbLogin";
            this.tsbLogin.Size = new System.Drawing.Size(23, 20);
            this.tsbLogin.Text = "";
            this.tsbLogin.ToolTipText = "Login";
            // 
            // tstbPassword
            // 
            this.tstbPassword.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.tstbPassword.Name = "tstbPassword";
            this.tstbPassword.Size = new System.Drawing.Size(100, 23);
            // 
            // tslPassword
            // 
            this.tslPassword.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.tslPassword.Font = new System.Drawing.Font("FontAwesome", 13F);
            this.tslPassword.Name = "tslPassword";
            this.tslPassword.Size = new System.Drawing.Size(80, 20);
            this.tslPassword.Text = "Contraseña:";
            // 
            // tstbUser
            // 
            this.tstbUser.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.tstbUser.Name = "tstbUser";
            this.tstbUser.Size = new System.Drawing.Size(100, 23);
            // 
            // tslUser
            // 
            this.tslUser.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.tslUser.Font = new System.Drawing.Font("FontAwesome", 13F);
            this.tslUser.Name = "tslUser";
            this.tslUser.Size = new System.Drawing.Size(58, 20);
            this.tslUser.Text = "Usuario:";
            // 
            // tstbRIF
            // 
            this.tstbRIF.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.tstbRIF.Name = "tstbRIF";
            this.tstbRIF.Size = new System.Drawing.Size(100, 23);
            // 
            // tslRIF
            // 
            this.tslRIF.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.tslRIF.Name = "tslRIF";
            this.tslRIF.Size = new System.Drawing.Size(26, 20);
            this.tslRIF.Text = "RIF:";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 23);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsslWebPageStatus});
            this.statusStrip1.Location = new System.Drawing.Point(0, 706);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(823, 22);
            this.statusStrip1.TabIndex = 10;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // tsslWebPageStatus
            // 
            this.tsslWebPageStatus.Name = "tsslWebPageStatus";
            this.tsslWebPageStatus.Size = new System.Drawing.Size(106, 17);
            this.tsslWebPageStatus.Text = "tsslWebPageStatus";
            // 
            // SBrowserProvincial
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(823, 728);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.tsUBrowser);
            this.Controls.Add(this.wbUmbrellaExplorer);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SBrowserProvincial";
            this.Text = "Umbrella Explorer";
            this.ControlAdded += new System.Windows.Forms.ControlEventHandler(this.SBrowserProvincial_ControlAdded);
            this.tsUBrowser.ResumeLayout(false);
            this.tsUBrowser.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.WebBrowser wbUmbrellaExplorer;
        private System.Windows.Forms.ToolStrip tsUBrowser;
        private System.Windows.Forms.ToolStripLabel tslUser;
        private System.Windows.Forms.ToolStripTextBox tstbUser;
        private System.Windows.Forms.ToolStripLabel tslPassword;
        private System.Windows.Forms.ToolStripTextBox tstbPassword;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripDropDownButton tsddbMenu;
        private System.Windows.Forms.ToolStripMenuItem tsmiLogout;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton tsbLogin;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel tsslWebPageStatus;
        private System.Windows.Forms.ToolStripMenuItem tsmiDates;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem tsmiRange;
        private System.Windows.Forms.ToolStripMenuItem tsmiDay;
        private System.Windows.Forms.ToolStripButton tsbHome;
        private System.Windows.Forms.ToolStripTextBox tstbRIF;
        private System.Windows.Forms.ToolStripLabel tslRIF;
        private System.Windows.Forms.ToolStripMenuItem tsmiAccount;
        private System.Windows.Forms.ToolStripMenuItem tsmiAccountBFI;
        private System.Windows.Forms.ToolStripMenuItem tsmiAccountTD;
    }
}

