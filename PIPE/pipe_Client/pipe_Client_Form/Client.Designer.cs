namespace pipe_Client_Form
{
    partial class Client
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.Send = new System.Windows.Forms.TextBox();
            this.Receive = new System.Windows.Forms.TextBox();
            this.sendButton = new System.Windows.Forms.Button();
            this.StartReceive = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // Send
            // 
            this.Send.Location = new System.Drawing.Point(106, 128);
            this.Send.Name = "Send";
            this.Send.Size = new System.Drawing.Size(419, 31);
            this.Send.TabIndex = 0;
            // 
            // Receive
            // 
            this.Receive.Location = new System.Drawing.Point(106, 232);
            this.Receive.Name = "Receive";
            this.Receive.Size = new System.Drawing.Size(419, 31);
            this.Receive.TabIndex = 1;
            // 
            // sendButton
            // 
            this.sendButton.Location = new System.Drawing.Point(532, 117);
            this.sendButton.Name = "sendButton";
            this.sendButton.Size = new System.Drawing.Size(124, 52);
            this.sendButton.TabIndex = 2;
            this.sendButton.Text = "Send";
            this.sendButton.UseVisualStyleBackColor = true;
            this.sendButton.Click += new System.EventHandler(this.sendButton_Click);
            // 
            // StartReceive
            // 
            this.StartReceive.Location = new System.Drawing.Point(518, 340);
            this.StartReceive.Name = "StartReceive";
            this.StartReceive.Size = new System.Drawing.Size(106, 50);
            this.StartReceive.TabIndex = 3;
            this.StartReceive.Text = "Start";
            this.StartReceive.UseVisualStyleBackColor = true;
            this.StartReceive.Click += new System.EventHandler(this.StartReceive_Click);
            // 
            // Client
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(686, 444);
            this.Controls.Add(this.StartReceive);
            this.Controls.Add(this.sendButton);
            this.Controls.Add(this.Receive);
            this.Controls.Add(this.Send);
            this.Name = "Client";
            this.Text = "Client";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox Send;
        private System.Windows.Forms.TextBox Receive;
        private System.Windows.Forms.Button sendButton;
        private System.Windows.Forms.Button StartReceive;
    }
}

