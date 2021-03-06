﻿using System.Windows.Forms;
namespace MagicMongoDBTool
{
    partial class ctlIndexCreate
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
            this.radAscendingKey = new System.Windows.Forms.RadioButton();
            this.radDescendingKey = new System.Windows.Forms.RadioButton();
            this.lblKeyName = new System.Windows.Forms.Label();
            this.radGeoSpatial = new System.Windows.Forms.RadioButton();
            this.cmbKeyName = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // radAscendingKey
            // 
            this.radAscendingKey.AutoSize = true;
            this.radAscendingKey.Checked = true;
            this.radAscendingKey.Location = new System.Drawing.Point(301, 6);
            this.radAscendingKey.Name = "radAscendingKey";
            this.radAscendingKey.Size = new System.Drawing.Size(43, 17);
            this.radAscendingKey.TabIndex = 1;
            this.radAscendingKey.TabStop = true;
            this.radAscendingKey.Text = "Asc";
            this.radAscendingKey.UseVisualStyleBackColor = true;
            // 
            // radDescendingKey
            // 
            this.radDescendingKey.AutoSize = true;
            this.radDescendingKey.Location = new System.Drawing.Point(356, 6);
            this.radDescendingKey.Name = "radDescendingKey";
            this.radDescendingKey.Size = new System.Drawing.Size(44, 17);
            this.radDescendingKey.TabIndex = 2;
            this.radDescendingKey.Text = "Des";
            this.radDescendingKey.UseVisualStyleBackColor = true;
            // 
            // lblKeyName
            // 
            this.lblKeyName.AutoSize = true;
            this.lblKeyName.Location = new System.Drawing.Point(8, 7);
            this.lblKeyName.Name = "lblKeyName";
            this.lblKeyName.Size = new System.Drawing.Size(55, 13);
            this.lblKeyName.TabIndex = 3;
            this.lblKeyName.Text = "IndexFiled";
            // 
            // radGeoSpatial
            // 
            this.radGeoSpatial.AutoSize = true;
            this.radGeoSpatial.Location = new System.Drawing.Point(415, 7);
            this.radGeoSpatial.Name = "radGeoSpatial";
            this.radGeoSpatial.Size = new System.Drawing.Size(45, 17);
            this.radGeoSpatial.TabIndex = 5;
            this.radGeoSpatial.TabStop = true;
            this.radGeoSpatial.Text = "Geo";
            this.radGeoSpatial.UseVisualStyleBackColor = true;
            // 
            // txtKeyName
            // 
            this.cmbKeyName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbKeyName.FormattingEnabled = true;
            this.cmbKeyName.Location = new System.Drawing.Point(82, 2);
            this.cmbKeyName.Name = "txtKeyName";
            this.cmbKeyName.Size = new System.Drawing.Size(213, 21);
            this.cmbKeyName.TabIndex = 6;
            // 
            // ctlIndexCreate
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.cmbKeyName);
            this.Controls.Add(this.radGeoSpatial);
            this.Controls.Add(this.lblKeyName);
            this.Controls.Add(this.radDescendingKey);
            this.Controls.Add(this.radAscendingKey);
            this.Name = "ctlIndexCreate";
            this.Size = new System.Drawing.Size(465, 30);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton radAscendingKey;
        private System.Windows.Forms.RadioButton radDescendingKey;
        private System.Windows.Forms.Label lblKeyName;
        private RadioButton radGeoSpatial;
        private ComboBox cmbKeyName;
    }
}
