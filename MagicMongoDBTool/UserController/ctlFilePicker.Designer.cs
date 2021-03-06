﻿namespace MagicMongoDBTool
{
    partial class ctlFilePicker
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
            this.cmdClearPath = new System.Windows.Forms.Button();
            this.lblTitle = new System.Windows.Forms.Label();
            this.txtLogPath = new System.Windows.Forms.TextBox();
            this.cmdBrowse = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // cmdClearPath
            // 
            this.cmdClearPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdClearPath.BackColor = System.Drawing.Color.Transparent;
            this.cmdClearPath.Location = new System.Drawing.Point(649, 0);
            this.cmdClearPath.Name = "cmdClearPath";
            this.cmdClearPath.Size = new System.Drawing.Size(75, 25);
            this.cmdClearPath.TabIndex = 10;
            this.cmdClearPath.Text = "Clear";
            this.cmdClearPath.UseVisualStyleBackColor = false;
            this.cmdClearPath.Click += new System.EventHandler(this.cmdClearPath_Click);
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.BackColor = System.Drawing.Color.Transparent;
            this.lblTitle.Location = new System.Drawing.Point(3, 8);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(27, 13);
            this.lblTitle.TabIndex = 9;
            this.lblTitle.Text = "Title";
            // 
            // txtLogPath
            // 
            this.txtLogPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtLogPath.BackColor = System.Drawing.Color.White;
            this.txtLogPath.Location = new System.Drawing.Point(36, 5);
            this.txtLogPath.Name = "txtLogPath";
            this.txtLogPath.ReadOnly = true;
            this.txtLogPath.Size = new System.Drawing.Size(523, 20);
            this.txtLogPath.TabIndex = 8;
            // 
            // cmdBrowse
            // 
            this.cmdBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdBrowse.BackColor = System.Drawing.Color.Transparent;
            this.cmdBrowse.Location = new System.Drawing.Point(568, 1);
            this.cmdBrowse.Name = "cmdBrowse";
            this.cmdBrowse.Size = new System.Drawing.Size(75, 24);
            this.cmdBrowse.TabIndex = 7;
            this.cmdBrowse.Text = "Browse...";
            this.cmdBrowse.UseVisualStyleBackColor = false;
            this.cmdBrowse.Click += new System.EventHandler(this.cmdBrowse_Click);
            // 
            // ctlFilePicker
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.cmdClearPath);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.txtLogPath);
            this.Controls.Add(this.cmdBrowse);
            this.Name = "ctlFilePicker";
            this.Size = new System.Drawing.Size(739, 33);
            this.Load += new System.EventHandler(this.ctlFilePicker_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button cmdClearPath;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.TextBox txtLogPath;
        private System.Windows.Forms.Button cmdBrowse;
    }
}
