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
using System.Xml;
using System.IO;
using System.Text;

namespace Apsoil
{
    public partial class AllAustralianSoils : System.Web.UI.Page
    {

        /// <summary>
        /// We're about to render the page - search for the soils and write names to the 
        /// response. The iPad soil app uses this method.
        /// </summary>
        protected void Page_PreRender(object sender, EventArgs e)
        {
            Response.Clear();
            Response.ContentType = "text/plain";

            string st = "";
            try
            {
                ApsoilWeb.Service SoilsDB = new Apsoil.ApsoilWeb.Service();
                ApsoilWeb.SoilBasicInfo[] soils = SoilsDB.AllAustralianSoils(null);


                
                if (soils == null || soils.Length == 0)
                    st = "No soils found";
                else
                {
                    st = "Soils:\r\n";
                    foreach (ApsoilWeb.SoilBasicInfo info in soils)
                        st += info.Name + " " + info.Latitude + " " + info.Longitude + "\r\n";
                }
            }
            catch (Exception err)
            {
                st = "Error: " + err.Message;
            }

            Response.BinaryWrite(System.Text.Encoding.UTF8.GetBytes(st));
        }   


        protected void Page_Load(object sender, EventArgs e)
        {

        }

    }
}
