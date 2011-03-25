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
using System.Xml.Linq;
using System.Xml;
using System.IO;

namespace Apsoil
   {
   public partial class GetSoil : System.Web.UI.Page
      {

      /// <summary>
      /// We're about to render the page - get the soil node and write it to the
      /// response.
      /// </summary>
      protected void Page_PreRender(object sender, EventArgs e)
         {
         Response.Clear();
         Response.ContentType = "text/plain";

         if (Request.QueryString["Name"] != null)
            {
            string SoilName = Request.QueryString["Name"];

            SoilsDB SoilsDB = new SoilsDB();
            SoilsDB.Open();
            XmlNode SoilNode = SoilsDB.GetSoil(SoilName);
            SoilsDB.Close();

            MemoryStream MemStream = new MemoryStream(10000);
            SoilNode.OwnerDocument.Save(MemStream);
            if (MemStream.ToArray() != null)
               Response.BinaryWrite(MemStream.ToArray());
            }
         }


      protected void Page_Load(object sender, EventArgs e)
         {

         }

      }
   }
