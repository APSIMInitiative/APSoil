using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.Data;
using ApsimFile;
using CSGeneral;
using System.IO;

namespace Apsoil
{
    public partial class Excel : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.QueryString["Name"] != null)
            {
                string FileName = Path.GetFileName(Request.QueryString["Name"]);

                Response.Clear();
                Response.AppendHeader("Content-Disposition", "inline; filename=" + FileName + ".csv");
                //Response.AppendHeader("Content-Disposition", "csv; Soils.csv");
                Response.Buffer = false;
                Response.ContentType = "application/vnd.ms-excel"; // text/plain
                
                string SoilName = Request.QueryString["Name"];

                ApsoilWeb.Service SoilsDB = new Apsoil.ApsoilWeb.Service();
                XmlDocument Doc = new XmlDocument();
                Doc.LoadXml("<soils>" + SoilsDB.SoilXML(SoilName) + "</soils>");

                DataTable Data = new DataTable("SoilData");
                int RowNumber = 0;
                SoilSpreadsheet.CreateTableFromData(Doc.DocumentElement, Data, "AllSoils", ref RowNumber);

                Response.Write(DataTableUtility.DataTableToCSV(Data, 0));
                Response.Flush();                 // send our content to the client browser.
                Response.SuppressContent = true;  // stops .net from writing it's stuff.
            }
        }

    }
}