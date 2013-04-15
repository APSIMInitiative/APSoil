using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Xml;
using ApsimFile;

namespace Apsoil
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        ApsoilWeb.Service SoilsDB = new Apsoil.ApsoilWeb.Service();

        protected void Page_Load(object sender, EventArgs e)
        {
            string[] AllSoils = SoilsDB.SoilNames();
            Array.Sort(AllSoils);
            Label.Text = "Number of soils: " + AllSoils.Length.ToString();
            foreach (string SoilName in AllSoils)
            {
                ListBox.Items.Add(SoilName);
            }
        }

        protected void ShowXMLClick(object sender, EventArgs e)
        {
            string SelectedName = ListBox.SelectedValue;
            Response.Redirect("http://www.apsim.info/ApsoilWeb/GetSoil.aspx?Name=" + SelectedName);
        }

        protected void ShowInfoClick(object sender, EventArgs e)
        {
            string SelectedName = ListBox.SelectedValue;

            Soil Soil = Soil.Create(SoilsDB.SoilXML(SelectedName));

            if (Soil != null)
            {
                InfoLabel.Text = "Latitude: " + Soil.Latitude +
                                 " Longitude: " + Soil.Longitude;
            }
            else
                InfoLabel.Text = "Invalid soil XML";
        }

        protected void KMLClick(object sender, EventArgs e)
        {
            Response.Redirect("http://www.apsim.info/ApsoilWeb/ApsoilKML.aspx?Mode=1");
        }

        protected void UploadClick(object sender, EventArgs e)
        {
            Response.Redirect("http://www.apsim.info/ApsoilWeb/UploadApsoilSoilsFile.aspx");
        }

        protected void DownloadClick(object sender, EventArgs e)
        {
            Response.Clear();
            Response.AppendHeader("Content-Disposition", "attachment; filename=" + "All.soils");
            Response.Buffer = false;
            Response.ContentType = "text/plain";

            ApsoilWeb.Service SoilsDB = new Apsoil.ApsoilWeb.Service();
            SoilsDB.Timeout = 360000;
            Response.Write(SoilsDB.SoilXMLAll());
            Response.Flush();                 // send our content to the client browser.
            Response.SuppressContent = true;  // stops .net from writing it's stuff.
            
        }

    }
}