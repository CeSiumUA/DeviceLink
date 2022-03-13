namespace DeviceLink.Server
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.startListenerButton = new System.Windows.Forms.Button();
            this.stopListenerButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // startListenerButton
            // 
            this.startListenerButton.Location = new System.Drawing.Point(12, 12);
            this.startListenerButton.Name = "startListenerButton";
            this.startListenerButton.Size = new System.Drawing.Size(242, 41);
            this.startListenerButton.TabIndex = 0;
            this.startListenerButton.Text = "Start listener";
            this.startListenerButton.UseVisualStyleBackColor = true;
            this.startListenerButton.Click += new System.EventHandler(this.startListenerButton_Click);
            // 
            // stopListenerButton
            // 
            this.stopListenerButton.Location = new System.Drawing.Point(12, 59);
            this.stopListenerButton.Name = "stopListenerButton";
            this.stopListenerButton.Size = new System.Drawing.Size(242, 41);
            this.stopListenerButton.TabIndex = 1;
            this.stopListenerButton.Text = "Stop listener";
            this.stopListenerButton.UseVisualStyleBackColor = true;
            this.stopListenerButton.Click += new System.EventHandler(this.stopListenerButton_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(266, 110);
            this.Controls.Add(this.stopListenerButton);
            this.Controls.Add(this.startListenerButton);
            this.Name = "Form1";
            this.Text = "DeviceLink Server";
            this.ResumeLayout(false);

        }

        #endregion

        private Button startListenerButton;
        private Button stopListenerButton;
    }
}