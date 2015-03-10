using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.IO;
using System.Xml;
using Graph;
using CSGeneral;
using ApsimFile;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Apsoil
{
    public partial class SoilChart : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Returns the content of the POST buffer as string
        /// </summary>
        /// <returns></returns>
        public static string FormBufferToString()
        {

            HttpRequest Request = HttpContext.Current.Request;

            if (Request.TotalBytes > 0)
                return Encoding.Default.GetString(Request.BinaryRead(Request.TotalBytes));

            return string.Empty;

        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            Response.ContentType = "image/png";
            Response.Buffer = true;
            byte[] ImageBytes = DrawChart();
            if (ImageBytes != null)
                Response.BinaryWrite(ImageBytes);
        }

        private byte[] DrawChart()
        {
            if (Request.QueryString["Name"] != null)
            {
                // Google Earth passes in a soil name parameter.
                string SoilName = Request.QueryString["Name"];
                ApsoilWeb.Service SoilsDB = new Apsoil.ApsoilWeb.Service();
                return SoilsDB.SoilChartPNG(SoilName);
            }
            else
            {
                // The iPad app passes in soil JSON rather than a name parameter.
                string json = FormBufferToString();
                XmlDocument doc = JsonConvert.DeserializeXmlNode(json);
                ApsoilWeb.Service SoilsDB = new Apsoil.ApsoilWeb.Service();
                byte[] bytes = SoilsDB.SoilChartPNGFromXML(doc.OuterXml);
                Context.Response.ContentType = "image/png";
                return bytes;
            }
        }


    }
}
