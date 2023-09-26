namespace Fic.XTB.FlowExecutionHistory.Forms
{
    partial class TriggerOutputsForm
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.dgvTriggerOutputsBody = new System.Windows.Forms.DataGridView();
            this.TriggerOutputsBodyName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.TriggerOutputsBodyValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.dgvTriggerOutputsHeaders = new System.Windows.Forms.DataGridView();
            this.TriggerOutputsHeadersName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.TriggerOutputsHeadersValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tableLayoutPanel1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTriggerOutputsBody)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTriggerOutputsHeaders)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.groupBox2, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.groupBox1, 0, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(12, 12);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1470, 426);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.dgvTriggerOutputsBody);
            this.groupBox2.Location = new System.Drawing.Point(738, 3);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(729, 420);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Body";
            // 
            // dgvTriggerOutputsBody
            // 
            this.dgvTriggerOutputsBody.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvTriggerOutputsBody.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvTriggerOutputsBody.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvTriggerOutputsBody.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.TriggerOutputsBodyName,
            this.TriggerOutputsBodyValue});
            this.dgvTriggerOutputsBody.Location = new System.Drawing.Point(6, 25);
            this.dgvTriggerOutputsBody.Name = "dgvTriggerOutputsBody";
            this.dgvTriggerOutputsBody.RowHeadersWidth = 62;
            this.dgvTriggerOutputsBody.RowTemplate.Height = 28;
            this.dgvTriggerOutputsBody.Size = new System.Drawing.Size(717, 389);
            this.dgvTriggerOutputsBody.TabIndex = 1;
            // 
            // TriggerOutputsBodyName
            // 
            this.TriggerOutputsBodyName.HeaderText = "Name";
            this.TriggerOutputsBodyName.MinimumWidth = 8;
            this.TriggerOutputsBodyName.Name = "TriggerOutputsBodyName";
            // 
            // TriggerOutputsBodyValue
            // 
            this.TriggerOutputsBodyValue.HeaderText = "Value";
            this.TriggerOutputsBodyValue.MinimumWidth = 8;
            this.TriggerOutputsBodyValue.Name = "TriggerOutputsBodyValue";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.dgvTriggerOutputsHeaders);
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(729, 420);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Headers";
            // 
            // dgvTriggerOutputsHeaders
            // 
            this.dgvTriggerOutputsHeaders.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvTriggerOutputsHeaders.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvTriggerOutputsHeaders.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvTriggerOutputsHeaders.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.TriggerOutputsHeadersName,
            this.TriggerOutputsHeadersValue});
            this.dgvTriggerOutputsHeaders.Location = new System.Drawing.Point(6, 25);
            this.dgvTriggerOutputsHeaders.Name = "dgvTriggerOutputsHeaders";
            this.dgvTriggerOutputsHeaders.RowHeadersWidth = 62;
            this.dgvTriggerOutputsHeaders.RowTemplate.Height = 28;
            this.dgvTriggerOutputsHeaders.Size = new System.Drawing.Size(717, 389);
            this.dgvTriggerOutputsHeaders.TabIndex = 0;
            // 
            // TriggerOutputsHeadersName
            // 
            this.TriggerOutputsHeadersName.HeaderText = "Name";
            this.TriggerOutputsHeadersName.MinimumWidth = 8;
            this.TriggerOutputsHeadersName.Name = "TriggerOutputsHeadersName";
            // 
            // TriggerOutputsHeadersValue
            // 
            this.TriggerOutputsHeadersValue.HeaderText = "Value";
            this.TriggerOutputsHeadersValue.MinimumWidth = 8;
            this.TriggerOutputsHeadersValue.Name = "TriggerOutputsHeadersValue";
            // 
            // TriggerOutputsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1494, 450);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "TriggerOutputsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "TriggerOutputsForm";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvTriggerOutputsBody)).EndInit();
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvTriggerOutputsHeaders)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.DataGridView dgvTriggerOutputsHeaders;
        private System.Windows.Forms.DataGridView dgvTriggerOutputsBody;
        private System.Windows.Forms.DataGridViewTextBoxColumn TriggerOutputsHeadersName;
        private System.Windows.Forms.DataGridViewTextBoxColumn TriggerOutputsHeadersValue;
        private System.Windows.Forms.DataGridViewTextBoxColumn TriggerOutputsBodyName;
        private System.Windows.Forms.DataGridViewTextBoxColumn TriggerOutputsBodyValue;
    }
}