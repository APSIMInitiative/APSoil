using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

public partial class ImageForm : Form
{
    public ImageForm()
    {
        InitializeComponent();
    }

    public void SetImage(byte[] bytes)
    {
        ImageConverter ic = new ImageConverter();
        this.Picture.Image = (Image)ic.ConvertFrom(bytes);
    }
}
