namespace DynaShape
{
    partial class DebugDashboard
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
            this.MainConsole = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // MainConsole
            // 
            this.MainConsole.Location = new System.Drawing.Point(-1, 0);
            this.MainConsole.Multiline = true;
            this.MainConsole.Name = "MainConsole";
            this.MainConsole.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.MainConsole.Size = new System.Drawing.Size(285, 262);
            this.MainConsole.TabIndex = 0;
            // 
            // DebugDashboard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.MainConsole);
            this.Name = "DebugDashboard";
            this.Text = "DebugDashboard";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal System.Windows.Forms.TextBox MainConsole;
    }
}