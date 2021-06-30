using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Data.SqlClient;
using System.Xml;
using System.IO;
using System.Web.UI.MobileControls;
using System.Collections.Generic;
using CSGeneral;
using Newtonsoft.Json;

namespace Apsoil
{
    /// <summary>
    /// Allows the user to upload a .soils file.
    /// </summary>
    public partial class UploadApsoilSoilsFile : System.Web.UI.Page
    {
        /// <summary>The soil path to upload a new XML for.</summary>
        private string pathToOverride;
        private bool userSoil = false;

        /// <summary>Handles the Load event of the Page control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.QueryString["UserSoils"] != null)
            {
                // User wants to upload user soils. Change label to reflect this.
                Label2.Text = "This form allows you to upload USER soils to apsimdev.apsim.info.";
                userSoil = true;
            }
        }

        /// <summary>
        /// User has clicked on upload button.
        /// </summary>
        protected void UploadButton_Click(object sender, EventArgs e)
        {
            using (ApsoilWeb.Service Soils = new Apsoil.ApsoilWeb.Service())
            {
                StreamReader In = new StreamReader(File1.FileContent);
                string contents = In.ReadToEnd();

                if (!userSoil)
                {
                    // Insert all soils into database.
                    Soils.UpdateAllSoils(contents);
                }
                else
                {
                    // Update a user soil.
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(contents);
                    foreach (var soil in XmlHelper.ChildNodes(doc.DocumentElement, "Soil"))
                    {
                        var soilParams = new ApsoilWeb.JsonSoilParam()
                        {
                            JSonSoil = JsonConvert.SerializeXmlNode(soil)
                        };
                        var ok = Soils.UpdateUserSoil(soilParams);
                    }
                }

                string[] AllSoils = Soils.SoilNames();
                SuccessLabel.Text = "Success. " + AllSoils.Length.ToString() + " soils in database.";
                SuccessLabel.Visible = true;
            }
        }



    }
}
