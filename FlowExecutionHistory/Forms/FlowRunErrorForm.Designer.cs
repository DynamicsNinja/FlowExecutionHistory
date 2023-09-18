namespace Fic.XTB.FlowExecutionHistory.Forms
{
    partial class FlowRunErrorForm
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
            this.lblError = new System.Windows.Forms.Label();
            this.tbError = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tbDetails = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // lblError
            // 
            this.lblError.AutoSize = true;
            this.lblError.Location = new System.Drawing.Point(12, 44);
            this.lblError.Name = "lblError";
            this.lblError.Size = new System.Drawing.Size(44, 20);
            this.lblError.TabIndex = 0;
            this.lblError.Text = "Error";
            // 
            // tbError
            // 
            this.tbError.Location = new System.Drawing.Point(76, 41);
            this.tbError.Name = "tbError";
            this.tbError.ReadOnly = true;
            this.tbError.Size = new System.Drawing.Size(712, 26);
            this.tbError.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 103);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(58, 20);
            this.label2.TabIndex = 2;
            this.label2.Text = "Details";
            // 
            // tbDetails
            // 
            this.tbDetails.Location = new System.Drawing.Point(76, 100);
            this.tbDetails.Multiline = true;
            this.tbDetails.Name = "tbDetails";
            this.tbDetails.ReadOnly = true;
            this.tbDetails.Size = new System.Drawing.Size(712, 234);
            this.tbDetails.TabIndex = 3;
            // 
            // FlowRunErrorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 357);
            this.Controls.Add(this.tbDetails);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tbError);
            this.Controls.Add(this.lblError);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FlowRunErrorForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Error Details";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblError;
        private System.Windows.Forms.TextBox tbError;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbDetails;
    }
}