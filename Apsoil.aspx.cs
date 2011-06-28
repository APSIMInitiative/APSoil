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
            Label.Text = "Number of soils: " + AllSoils.Length.ToString();
            foreach (string SoilName in AllSoils)
            {
                ListBox.Items.Add(SoilName);
            }
        }

        protected void Button_Click(object sender, EventArgs e)
        {
            string SelectedName = ListBox.SelectedValue;
            Response.Redirect("http://www.apsim.info/ApsoilWeb/GetSoil.aspx?Name=" + SelectedName);
        }

        protected void Button2_Click(object sender, EventArgs e)
        {
            string SelectedName = ListBox.SelectedValue;

            XmlDocument Doc = new XmlDocument();
            Doc.LoadXml(SoilsDB.SoilXML(SelectedName));
            XmlNode SoilNode = Doc.DocumentElement;

            if (SoilNode != null)
            {
                double Latitude;
                double Longitude;
                if (double.TryParse(Soil.Get(SoilNode, "Latitude").Value, out Latitude) &&
                    double.TryParse(Soil.Get(SoilNode, "Longitude").Value, out Longitude))
                {
                    InfoLabel.Text = "Latitude: " + Soil.Get(SoilNode, "Latitude").Value +
                                     " Longitude: " + Soil.Get(SoilNode, "Longitude").Value;
                }
                else
                    InfoLabel.Text = "Invalid latitude or longitude";
            }
            else
                InfoLabel.Text = "Invalid soil XML";
        }

        protected void Button3_Click(object sender, EventArgs e)
        {
            Response.Redirect("http://www.apsim.info/ApsoilWeb/ApsoilKML.aspx?Mode=1");
        }

        protected void Button4_Click(object sender, EventArgs e)
        {
            Response.Redirect("http://www.apsim.info/ApsoilWeb/UploadSoilsFile.aspx");
        }
    }
}