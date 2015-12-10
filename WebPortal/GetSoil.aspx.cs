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

namespace Apsoil
{
    public partial class GetSoil : System.Web.UI.Page
    {

        /// <summary>
        /// We're about to render the page - search for the soils and write names to the 
        /// response. The iPad soil app uses this method.
        /// </summary>
        protected void Page_PreRender(object sender, EventArgs e)
        {
            Response.Clear();
            Response.ContentType = "text/plain";
            Response.ContentEncoding = System.Text.Encoding.UTF8;

            if (Request.QueryString["Name"] != null)
            {
                string SoilName = Request.QueryString["Name"];

                ApsoilWeb.Service SoilsDB = new Apsoil.ApsoilWeb.Service();
                XmlDocument Doc = new XmlDocument();
                Doc.LoadXml(SoilsDB.SoilXML(SoilName));

                MemoryStream MemStream = new MemoryStream(10000);
                Doc.Save(MemStream);
                if (MemStream.ToArray() != null)
                    Response.BinaryWrite(MemStream.ToArray());
            }
            else if (Request.QueryString["NameJSON"] != null)
            {
                string SoilName = Request.QueryString["NameJSON"];

                ApsoilWeb.Service SoilsDB = new Apsoil.ApsoilWeb.Service();
                Response.Write(SoilsDB.SoilAsJson(SoilName));
            }
            Response.End();
        }


        protected void Page_Load(object sender, EventArgs e)
        {

        }

    }
}
