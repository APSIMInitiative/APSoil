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
using KML_22_Beta1;
using KML_22_Beta1.KML_22_Beta1_Utils;
using CSGeneral;
using System.IO;
using ApsimFile;
using System.Web.UI.MobileControls;
using System.Collections.Generic;

namespace Apsoil
   {
   public partial class UploadSoilsFile : System.Web.UI.Page
      {
      private SoilsDB SoilsDB = new SoilsDB();
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
         SoilsDB.Open();

         // Remove all existing soils.
         SoilsDB.DeleteAllSoils();

         // Load in the users uploaded .soils (XML) file.
         XmlDocument Doc = new XmlDocument();
         Doc.Load(File1.FileContent);

         // Insert all soils into database.
         InsertFolderIntoDB(Doc.DocumentElement);

         // Close database connection.
         SoilsDB.Close();

         SuccessLabel.Visible = true;
         }





      /// <summary>
      /// Recursively insert all soils into database.
      /// </summary>
      private void InsertFolderIntoDB(XmlNode FolderNode)
         {
         foreach (XmlNode SoilNode in XmlHelper.ChildNodes(FolderNode, "Soil"))
            SoilsDB.AddSoil(XmlHelper.FullPath(SoilNode), SoilNode.OuterXml);

         foreach (XmlNode ChildFolderNode in XmlHelper.ChildNodes(FolderNode, "Folder"))
            InsertFolderIntoDB(ChildFolderNode);
         }



      }
   }
