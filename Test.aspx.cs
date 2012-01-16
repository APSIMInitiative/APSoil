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
using System.Web.UI.MobileControls;
using System.Collections.Generic;
using System.Xml;
using CSGeneral;
using Graph;
using ApsimFile;
using System.IO;
using System.Drawing;

namespace Apsoil
   {
   public partial class Test : System.Web.UI.Page
      {
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
         // Create an instance of the APSoil web service.
         ApsoilWeb.Service SoilsDB = new Apsoil.ApsoilWeb.Service();

         // Get a list of all soil names from the web service.
         // These soil names are of the form:
         //   /Soils/Australia/Queensland/Borders and Western Downs/Red Chromosol (Billa Billa No066)
         // YP will have to parse these names into country, state and region. 
         string[] SoilNames = SoilsDB.SoilNames();

         if (SoilNames.Length > 0)
            {
            string SoilName = SoilNames[0];
            string CropName = "wheat";

            // Make sure we can convert from old sample xml to new sample xml.
            string OldSoilSampleXML = "<soilsample name=\"Soil sample 1\">" +
                                   "<profile>" +
                                   "  <layer>" +
                                   "    <thickness>100</thickness>" +
                                   "    <no3>10.33</no3>" +
                                   "    <nh4>2</nh4>" +
                                   "    <sw>0.0689</sw>" +
                                   "  </layer>" +
                                   "  <layer>" +
                                   "    <thickness>100</thickness>" +
                                   "    <no3>9</no3>" +
                                   "    <nh4>0</nh4>" +
                                   "    <sw>0.1311</sw>" +
                                   "  </layer>" +
                                   "  <layer>" +
                                   "    <thickness>200</thickness>" +
                                   "    <no3>9</no3>" +
                                   "    <nh4>0</nh4>" +
                                   "    <sw>0.19</sw>" +
                                   "  </layer>" +
                                   "  <layer>" +
                                   "    <thickness>200</thickness>" +
                                   "    <no3>9</no3>" +
                                   "    <nh4>0</nh4>" +
                                   "    <sw>0.225</sw>" +
                                   "  </layer>" +
                                   "  <layer>" +
                                   "    <thickness>200</thickness>" +
                                   "    <no3>2.67</no3>" +
                                   "    <nh4>0</nh4>" +
                                   "    <sw>0.1726</sw>" +
                                   "  </layer>" +
                                   "</profile>" +
                                   "</soilsample>";
            string NewSoilSampleXML = SoilsDB.ConvertSoilSampleXML(OldSoilSampleXML);

            // Make sure we can create a soil sample 1.
            string[] Depths = new string[] { "0-10", "10-40", "40-70", "70-100" };
            double[] SW = new double[] { 0.109, 0.156, 0.243, 0.210 };  // not % but mm/mm - divide percent by 100
            double[] NO3 = new double[] { 9, 19, 9, 2 };
            double[] NH4 = new double[] { 6, 6, 6, 4 };
            string SWUnits = "mm/mm";   // This is volumetic - alternative units: "grav. mm/mm";
            string SoilSample1XML = SoilsDB.CreateSoilSample1XML(new DateTime(2011, 12, 25), Depths, SWUnits, SW, NO3, NH4);

            // Make sure we can create a soil sample 2.
            double[] OC = new double[] { 1.72, 0.54, 0.21, 0.16 };
            double[] EC = new double[] { 0.117, 0.242, 0.652, 0.908 };
            double[] PH = new double[] { 7, 8, 8.8, 9 };
            double[] CL = new double[] { 93.6, 147.5, 609.8, 891.8 };
            string SoilSample2XML = SoilsDB.CreateSoilSample2XML(new DateTime(2011, 12, 25), Depths, OC, EC, PH, CL);

            // Make sure paw and pawc work.
            double PAW = SoilsDB.PAW(SoilName, NewSoilSampleXML, CropName);
            double PAWC = SoilsDB.PAWC(SoilName, CropName);

            // Get a soil sample graph in PNG format.

            return SoilsDB.SoilChartWithSamplePNG(SoilName, SoilSample1XML);
            }
         else
            return null;
         }



      }
   }
