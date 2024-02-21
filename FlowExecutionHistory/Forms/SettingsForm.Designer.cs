namespace Fic.XTB.FlowExecutionHistory.Forms
{
    partial class SettingsForm
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
            this.gbStyle = new System.Windows.Forms.GroupBox();
            this.cbShowFreindlyCorrIds = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cbFlowColors = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.gbColumns = new System.Windows.Forms.GroupBox();
            this.cbShowErrorColumn = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.gbStyle.SuspendLayout();
            this.gbColumns.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbStyle
            // 
            this.gbStyle.Controls.Add(this.cbShowFreindlyCorrIds);
            this.gbStyle.Controls.Add(this.label2);
            this.gbStyle.Controls.Add(this.cbFlowColors);
            this.gbStyle.Controls.Add(this.label1);
            this.gbStyle.Location = new System.Drawing.Point(12, 12);
            this.gbStyle.Name = "gbStyle";
            this.gbStyle.Size = new System.Drawing.Size(369, 126);
            this.gbStyle.TabIndex = 0;
            this.gbStyle.TabStop = false;
            this.gbStyle.Text = "Style";
            // 
            // cbShowFreindlyCorrIds
            // 
            this.cbShowFreindlyCorrIds.AutoSize = true;
            this.cbShowFreindlyCorrIds.Location = new System.Drawing.Point(333, 83);
            this.cbShowFreindlyCorrIds.Name = "cbShowFreindlyCorrIds";
            this.cbShowFreindlyCorrIds.Size = new System.Drawing.Size(22, 21);
            this.cbShowFreindlyCorrIds.TabIndex = 4;
            this.cbShowFreindlyCorrIds.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 82);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(207, 20);
            this.label2.TabIndex = 3;
            this.label2.Text = "Show frendly correlation IDs";
            // 
            // cbFlowColors
            // 
            this.cbFlowColors.AutoSize = true;
            this.cbFlowColors.Location = new System.Drawing.Point(333, 47);
            this.cbFlowColors.Name = "cbFlowColors";
            this.cbFlowColors.Size = new System.Drawing.Size(22, 21);
            this.cbFlowColors.TabIndex = 2;
            this.cbFlowColors.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 46);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(321, 20);
            this.label1.TabIndex = 1;
            this.label1.Text = "Use color coded flow names in flow runs grid";
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(306, 251);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 30);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(225, 251);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 30);
            this.btnSave.TabIndex = 2;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // gbColumns
            // 
            this.gbColumns.Controls.Add(this.cbShowErrorColumn);
            this.gbColumns.Controls.Add(this.label4);
            this.gbColumns.Location = new System.Drawing.Point(12, 144);
            this.gbColumns.Name = "gbColumns";
            this.gbColumns.Size = new System.Drawing.Size(369, 101);
            this.gbColumns.TabIndex = 5;
            this.gbColumns.TabStop = false;
            this.gbColumns.Text = "Columns";
            // 
            // cbShowErrorColumn
            // 
            this.cbShowErrorColumn.AutoSize = true;
            this.cbShowErrorColumn.Location = new System.Drawing.Point(333, 47);
            this.cbShowErrorColumn.Name = "cbShowErrorColumn";
            this.cbShowErrorColumn.Size = new System.Drawing.Size(22, 21);
            this.cbShowErrorColumn.TabIndex = 2;
            this.cbShowErrorColumn.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 46);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(149, 20);
            this.label4.TabIndex = 1;
            this.label4.Text = "Show error columns";
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(392, 291);
            this.Controls.Add(this.gbColumns);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.gbStyle);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Settings";
            this.gbStyle.ResumeLayout(false);
            this.gbStyle.PerformLayout();
            this.gbColumns.ResumeLayout(false);
            this.gbColumns.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gbStyle;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox cbFlowColors;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.CheckBox cbShowFreindlyCorrIds;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox gbColumns;
        private System.Windows.Forms.CheckBox cbShowErrorColumn;
        private System.Windows.Forms.Label label4;
    }
}