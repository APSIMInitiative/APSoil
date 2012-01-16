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

namespace Apsoil
   {
   public partial class SoilChart : System.Web.UI.Page
      {
      protected void Page_Load(object sender, EventArgs e)
         {

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
            string SoilName = Request.QueryString["Name"];

            ApsoilWeb.Service SoilsDB = new Apsoil.ApsoilWeb.Service();

            return SoilsDB.SoilChartPNG(SoilName);
            }
         else
            return null;
         }


      }
   }
