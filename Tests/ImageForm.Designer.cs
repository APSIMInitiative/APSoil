
partial class ImageForm
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
        this.Picture = new System.Windows.Forms.PictureBox();
        ((System.ComponentModel.ISupportInitialize)(this.Picture)).BeginInit();
        this.SuspendLayout();
        // 
        // Picture
        // 
        this.Picture.Dock = System.Windows.Forms.DockStyle.Fill;
        this.Picture.Location = new System.Drawing.Point(0, 0);
        this.Picture.Name = "Picture";
        this.Picture.Size = new System.Drawing.Size(322, 308);
        this.Picture.TabIndex = 0;
        this.Picture.TabStop = false;
        // 
        // ImageForm
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(322, 308);
        this.Controls.Add(this.Picture);
        this.Name = "ImageForm";
        this.Text = "ImageForm";
        ((System.ComponentModel.ISupportInitialize)(this.Picture)).EndInit();
        this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.PictureBox Picture;
}
