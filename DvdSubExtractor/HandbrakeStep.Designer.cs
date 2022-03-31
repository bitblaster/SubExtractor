namespace DvdSubExtractor
{
    partial class HandbrakeStep
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
            if(disposing && (components != null))
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
            this.optionsButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // optionsButton
            // 
            this.optionsButton.Location = new System.Drawing.Point(430, 18);
            this.optionsButton.Name = "optionsButton";
            this.optionsButton.Size = new System.Drawing.Size(203, 33);
            this.optionsButton.TabIndex = 0;
            this.optionsButton.Text = "Handbrake Options...";
            this.optionsButton.UseVisualStyleBackColor = true;
            this.optionsButton.Click += new System.EventHandler(this.optionsButton_Click);
            // 
            // HandbrakeStep
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.optionsButton);
            this.Name = "HandbrakeStep";
            this.Size = new System.Drawing.Size(720, 562);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button optionsButton;
    }
}
