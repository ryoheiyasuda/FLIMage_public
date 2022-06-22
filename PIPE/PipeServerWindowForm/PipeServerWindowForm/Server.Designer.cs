namespace PipeServerWindowForm
{
    partial class Server
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
            this.ReceiveText = new System.Windows.Forms.TextBox();
            this.sendButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // Send
            // 
            this.Send.Location = new System.Drawing.Point(133, 136);
            this.Send.Name = "Send";
            this.Send.Size = new System.Drawing.Size(363, 31);
            this.Send.TabIndex = 0;
            // 
            // ReceiveText
            // 
            this.ReceiveText.Location = new System.Drawing.Point(133, 263);
            this.ReceiveText.Name = "ReceiveText";
            this.ReceiveText.Size = new System.Drawing.Size(363, 31);
            this.ReceiveText.TabIndex = 1;
            // 
            // sendButton
            // 
            this.sendButton.Location = new System.Drawing.Point(237, 186);
            this.sendButton.Name = "sendButton";
            this.sendButton.Size = new System.Drawing.Size(358, 38);
            this.sendButton.TabIndex = 2;
            this.sendButton.Text = "Send!";
            this.sendButton.UseVisualStyleBackColor = true;
            this.sendButton.Click += new System.EventHandler(this.sendButton_Click);
            // 
            // Server
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(607, 407);
            this.Controls.Add(this.sendButton);
            this.Controls.Add(this.ReceiveText);
            this.Controls.Add(this.Send);
            this.Name = "Server";
            this.Text = "Server";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Server_FormClosing);
            this.Load += new System.EventHandler(this.Server_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox Send;
        private System.Windows.Forms.TextBox ReceiveText;
        private System.Windows.Forms.Button sendButton;
    }
}

