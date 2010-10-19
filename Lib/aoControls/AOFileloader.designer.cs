namespace AOControls
{
    partial class AOFileloader
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.lnkFileSrc = new System.Windows.Forms.LinkLabel();
            this.aoButton1 = new AOControls.AOButton();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.Controls.Add(this.lnkFileSrc, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.aoButton1, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(152, 23);
            this.tableLayoutPanel1.TabIndex = 0;
            this.tableLayoutPanel1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Control_MouseDown);
            // 
            // lnkFileSrc
            // 
            this.lnkFileSrc.AutoSize = true;
            this.lnkFileSrc.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lnkFileSrc.Location = new System.Drawing.Point(5, 5);
            this.lnkFileSrc.Margin = new System.Windows.Forms.Padding(5);
            this.lnkFileSrc.Name = "lnkFileSrc";
            this.lnkFileSrc.Size = new System.Drawing.Size(112, 13);
            this.lnkFileSrc.TabIndex = 0;
            this.lnkFileSrc.TabStop = true;
            this.lnkFileSrc.Text = "Document Link";
            this.lnkFileSrc.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lnkFileSrc.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Control_MouseDown);
            this.lnkFileSrc.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkFileSrc_LinkClicked);
            // 
            // aoButton1
            // 
            this.aoButton1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.aoButton1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.aoButton1.Location = new System.Drawing.Point(125, 3);
            this.aoButton1.Name = "aoButton1";
            this.aoButton1.Size = new System.Drawing.Size(24, 17);
            this.aoButton1.TabIndex = 1;
            this.aoButton1.Text = "...";
            this.aoButton1.UseVisualStyleBackColor = true;
            this.aoButton1.Click += new System.EventHandler(this.aoButton1_Click);
            this.aoButton1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Control_MouseDown);
            // 
            // AOFileloader
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "AOFileloader";
            this.Size = new System.Drawing.Size(152, 100);
            this.Load += new System.EventHandler(this.AOFileloader_Load);
            this.Leave += new System.EventHandler(this.AOFileloader_Leave);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.AOFileloader_KeyUp);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.AOFileloader_KeyPress);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.AOFileloader_KeyDown);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.LinkLabel lnkFileSrc;
        private AOButton aoButton1;
    }
}
