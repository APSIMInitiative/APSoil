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
using System.IO;
using System.Xml;
using Graph;
using CSGeneral;
using ApsimFile;
using System.Collections.Generic;
using Steema.TeeChart.Styles;

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

            SoilsDB SoilsDB = new SoilsDB();
            SoilsDB.Open();
            XmlNode SoilNode = SoilsDB.GetSoil(SoilName);
            XmlNode WaterNode = XmlHelper.Find(SoilNode, "Water");
            SoilsDB.Close();

            DataTable Table = new DataTable();
            Table.TableName = "Water";
            List<string> VariableNames = Soil.ValidVariablesForProfileNode(WaterNode);
            VariableNames.RemoveAt(0);
            VariableNames.Insert(0, "DepthMidPoints (mm)");
            foreach (string Crop in Soil.Crops(SoilNode))
               VariableNames.Add(Crop + " LL(mm/mm)");

            Soil.WriteToTable(SoilNode, Table, VariableNames);

            SoilGraphUI Graph = new SoilGraphUI();
            Graph.SoilNode = SoilNode;
            Graph.OnLoad(null, "", WaterNode.OuterXml);
            Graph.DataSources.Add(Table);
            Graph.OnRefresh();           
            
            // Make first 3 LL series active.
            int Count = 0;
            foreach (Series S in Graph.Chart.Series)
               {
               if (S.Title.Contains(" LL"))
                  {
                  S.Active = true;
                  Count++;
                  if (Count == 3)
                     break;
                  }
               }
            Graph.Chart.Legend.CheckBoxes = false;

            MemoryStream MemStream = new MemoryStream(10000);
            Graph.Chart.Export.Image.PNG.Height = 450;
            Graph.Chart.Export.Image.PNG.Width = 350;
            Graph.Chart.Export.Image.PNG.Save(MemStream);

            return MemStream.ToArray();
            }
         else
            return null;
         }


      }
   }
