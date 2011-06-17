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

namespace Apsoil
   {
   public partial class UploadSoilsFile : System.Web.UI.Page
      {
      protected void Page_Load(object sender, EventArgs e)
         {

         }

      protected override void OnLoad(EventArgs e)
         {
         base.OnLoad(e);
         }

      /// <summary>
      /// User has clicked on upload button.
      /// </summary>
      protected void UploadButton_Click(object sender, EventArgs e)
         {
         ApsoilWeb.Service Soils = new Apsoil.ApsoilWeb.Service();

         // Insert all soils into database.
         Soils.UpdateAllSoils(File1.FileContent.ToString());

         SuccessLabel.Visible = true;
         }



      }
   }
