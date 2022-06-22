namespace PhysiologyCSharp
{
    partial class MC700BCommander
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
            this.valueTextBox = new System.Windows.Forms.TextBox();
            this.ReadButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // valueTextBox
            // 
            this.valueTextBox.Location = new System.Drawing.Point(11, 44);
            this.valueTextBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.valueTextBox.Multiline = true;
            this.valueTextBox.Name = "valueTextBox";
            this.valueTextBox.Size = new System.Drawing.Size(266, 460);
            this.valueTextBox.TabIndex = 0;
            // 
            // ReadButton
            // 
            this.ReadButton.Location = new System.Drawing.Point(216, 11);
            this.ReadButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.ReadButton.Name = "ReadButton";
            this.ReadButton.Size = new System.Drawing.Size(61, 29);
            this.ReadButton.TabIndex = 1;
            this.ReadButton.Text = "Read";
            this.ReadButton.UseVisualStyleBackColor = true;
            this.ReadButton.Click += new System.EventHandler(this.ReadButton_Click);
            // 
            // MC700BCommander
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(288, 515);
            this.Controls.Add(this.ReadButton);
            this.Controls.Add(this.valueTextBox);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "MC700BCommander";
            this.Text = "MC700BCommander";
            this.Load += new System.EventHandler(this.MC700BCommander_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox valueTextBox;
        private System.Windows.Forms.Button ReadButton;
    }
}

