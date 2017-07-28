namespace VoxelRenderTest
{
    partial class RenderScreen
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.DrawArea = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // DrawArea
            // 
            this.DrawArea.Location = new System.Drawing.Point(12, 12);
            this.DrawArea.Margin = new System.Windows.Forms.Padding(12);
            this.DrawArea.MaximumSize = new System.Drawing.Size(512, 512);
            this.DrawArea.MinimumSize = new System.Drawing.Size(512, 512);
            this.DrawArea.Name = "DrawArea";
            this.DrawArea.Size = new System.Drawing.Size(512, 512);
            this.DrawArea.TabIndex = 0;
            this.DrawArea.Paint += new System.Windows.Forms.PaintEventHandler(this.DrawArea_Paint);
            this.DrawArea.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DrawArea_MouseDown);
            this.DrawArea.MouseEnter += new System.EventHandler(this.DrawArea_MouseEnter);
            this.DrawArea.MouseMove += new System.Windows.Forms.MouseEventHandler(this.DrawArea_MouseMove);
            this.DrawArea.MouseUp += new System.Windows.Forms.MouseEventHandler(this.DrawArea_MouseUp);
            this.DrawArea.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.DrawArea_MouseWheel);
            // 
            // RenderScreen
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(619, 608);
            this.Controls.Add(this.DrawArea);
            this.Name = "RenderScreen";
            this.Text = "Ray Tracing";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel DrawArea;
    }
}

