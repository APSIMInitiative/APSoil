using System;
using System.Web;
using System.Xml;
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
            Response.Flush();
            Response.End();
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
                if (json == null || json == string.Empty)
                    throw new Exception("No JSON specified in call to SoilChart.aspx");
                XmlDocument doc = JsonConvert.DeserializeXmlNode(json);
                if (doc == null)
                    throw new Exception("Invalid JSON passed to SoilChart.aspx");
                ApsoilWeb.Service SoilsDB = new Apsoil.ApsoilWeb.Service();
                byte[] bytes = SoilsDB.SoilChartPNGFromXML(doc.OuterXml);
                Context.Response.ContentType = "image/png";
                return bytes;
            }
        }


    }
}
