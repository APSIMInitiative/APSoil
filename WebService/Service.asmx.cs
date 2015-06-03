using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Data.SqlClient;
using System.Xml;

using Steema.TeeChart.Styles;
using System.IO;
using System.Drawing;
using ApsimFile;
using CSGeneral;
using Graph;
using System.Text;
using System.Web.Script.Services;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Xml.Serialization;

namespace Apsoil
{
    /// <summary>
    /// Summary description for Service1
    /// </summary>
    [WebService(Namespace = "http://www.apsim.info/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ToolboxItem(false)]
    [ScriptService]
    public class Service : System.Web.Services.WebService
    {
        private string TableName = "AllSoils";

        /// <summary>
        /// Get a list of names from table. YieldProphet calls this.
        /// </summary>
        [WebMethod]
        public List<string> SoilNames()
        {
            return AllSoilNames(true);
        }

        /// <summary>
        /// Get a list of names from table. Google Earth calls this.
        /// </summary>
        [WebMethod]
        public List<string> AllSoilNames(bool IncludeUserSoils)
        {
            SqlConnection Connection = Open();
            List<string> _SoilNames = new List<string>();
            SqlDataReader Reader = null;
            try
            {
                SqlCommand Command;
                if (IncludeUserSoils)
                    Command = new SqlCommand("SELECT Name FROM " + TableName, Connection);
                else
                    Command = new SqlCommand("SELECT Name FROM " + TableName + " WHERE IsApsoil=1", Connection);

                Reader = Command.ExecuteReader();
                while (Reader.Read())
                    _SoilNames.Add(Reader["Name"].ToString());
                Reader.Close();
            }
            catch (Exception err)
            {
                Reader.Close();
                Connection.Close();
                throw err;
            }
            Connection.Close();
            return _SoilNames;
        }

        /// <summary>
        /// Update all soils to the specified .soils content.
        /// </summary>
        [WebMethod]
        public void UpdateAllSoils(string Contents)
        {
            SqlConnection Connection = Open();
            try
            {

            // Delete all soils.
            SqlCommand Cmd = new SqlCommand("DELETE FROM " + TableName + " WHERE IsApsoil = 1", Connection);
            Cmd.ExecuteNonQuery();

            // Load in the XML
            XmlDocument Doc = new XmlDocument();
            Doc.LoadXml(Contents);

            // Insert all soils into database.
            InsertFolderIntoDB(Connection, Doc.DocumentElement);
            }
            catch (Exception err)
            {
                Connection.Close();
                throw err;
            }
            Connection.Close();
        }

        /// <summary>
        /// Return the soil node for the specified soil. Will return
        /// null if soil was found.
        /// </summary>
        [WebMethod]
        public string SoilXML(string Name)
        {
            string ReturnString = "";
            SqlConnection Connection = Open();
            SqlDataReader Reader = null;
            try
            {
                SqlCommand Command = new SqlCommand("SELECT XML FROM " + TableName + " WHERE Name = @Name", Connection);
                Command.Parameters.Add(new SqlParameter("@Name", Name));
                Reader = Command.ExecuteReader();
                if (Reader.Read())
                {
                    XmlDocument Doc = new XmlDocument();
                    Doc.LoadXml(Reader["XML"].ToString());
                    if (Doc.DocumentElement.Name == "soil")
                    {
                        // old soil format - convert to new.
                        ReturnString = ConvertOldXmlToNew(Doc.OuterXml);
                    }
                    else
                        ReturnString = Doc.OuterXml;
                }
            }
            finally
            {
                Reader.Close();
                Connection.Close();
            }
            return ReturnString;
        }

        /// <summary>
        /// Return info about a soil.
        /// </summary>
        [WebMethod]
        public SoilInfo GetSoilInfo(string SoilName)
        {
            Soil Soil = Soil.Create(SoilXML(SoilName));

            SoilInfo SoilInfo = new SoilInfo();
            SoilInfo.Name = Soil.Name;
            SoilInfo.Description = Soil.DataSource;
            SoilInfo.SoilType = Soil.SoilType;
            SoilInfo.Latitude = Soil.Latitude;
            SoilInfo.Longitude = Soil.Longitude;
            SoilInfo.ASCOrder = Soil.ASCOrder;
            SoilInfo.ASCSubOrder = Soil.ASCSubOrder;
            SoilInfo.Site = Soil.Site;
            SoilInfo.Region = Soil.Region;
            SoilInfo.NearestTown = Soil.NearestTown;
            return SoilInfo;
        }

        /// <summary>
        /// Return info about a soil.
        /// </summary>
        [WebMethod]
        public SoilAnalysisInfo GetSoilAnalysisInfo(string SoilName)
        {
            Soil Soil = Soil.Create(SoilXML(SoilName));
            SoilAnalysisInfo Analysis = new SoilAnalysisInfo();
            Analysis.Thickness = Soil.Analysis.Thickness;
            Analysis.Texture = Soil.Analysis.Texture;
            Analysis.EC = Soil.Analysis.EC;
            Analysis.PH = Soil.Analysis.PH;
            Analysis.CL = Soil.Analysis.CL;
            Analysis.Boron = Soil.Analysis.Boron;
            Analysis.ESP = Soil.Analysis.ESP;
            Analysis.AL = Soil.Analysis.Al;


            if (StringManip.Contains(Soil.CropNames, "wheat"))
            {
                Analysis.WheatLL = Soil.Crop("Wheat").LL;
                Analysis.WheatXF = Soil.Crop("Wheat").XF;
            }
            Analysis.LL15 = Soil.LL15;
            Analysis.DUL = Soil.DUL;
            return Analysis;
        }
        
        /// <summary>
        /// Returns a list of all soil types in the APSoil repository.
        /// </summary>
        /// <returns></returns>
        [WebMethod]
        public string[] SoilTypes()
        {
            List<string> Types = new List<string>();
            XmlDocument Doc = new XmlDocument();
            foreach (string SoilName in SoilNames())
            {
                Soil Soil = Soil.Create(SoilXML(SoilName));
                string SoilType = Soil.SoilType;
                if (SoilType != "" && SoilType != null)
                    Types.Add(SoilType);
            }
            return Types.ToArray();
        }

        /// <summary>
        /// Return the PAW (SW - CropLL) for the specified soil.
        /// </summary>
        [WebMethod]
        public double PAW(string SoilName, double[] Thickness, double[] SW, bool IsGravimetric, string CropName)
        {
            Soil Soil = Soil.Create(SoilXML(SoilName));
            if (!StringManip.Contains(Soil.CropNames, CropName))
                CropName = "Wheat";

            RemoveMissingValues(ref Thickness, ref SW);
            Soil.Samples.Add(new Sample() { Thickness = Thickness, SW = SW });
            if (IsGravimetric)
                Soil.Samples[0].SWUnits = Sample.SWUnitsEnum.Gravimetric;

            try
            {
                double[] PAWCmm = MathUtility.Multiply(Soil.PAWCrop(CropName), Soil.Thickness);
                double PAWmm = MathUtility.Sum(PAWCmm);
                return PAWmm;
            }
            catch (Exception)
            {
                // Crop doesn't exist.
                return 0.0;
            }
        }

        /// <summary>
        /// This method parses the sample xml passed in and extracts an array of numbers from it.
        /// </summary>
        private void GetSWFromSampleXML(string Xml, string VariableName, 
                                        out double[] Values, out string Units)
        {
            // Load in the sample XML
            XmlDocument Doc = new XmlDocument();
            Doc.LoadXml(Xml);

            // Find sample child.
            XmlNode Node = Doc.DocumentElement;
            if (Node.Name != "Sample")
                Node = XmlHelper.Find(Doc.DocumentElement, "Sample");
            if (Node == null)
                throw new Exception("Cannot find a <Sample> node in: " + Xml);

            // Loop through all <Layer> nodes and get each SW value.
            Units = "";
            List<double> AllValues = new List<double>();
            foreach (XmlNode Layer in XmlHelper.ChildNodes(Node, "Layer"))
            {
                XmlNode ValueNode = XmlHelper.Find(Layer, VariableName);

                string Value = "";
                string Code = "";
                if (ValueNode != null)
                {
                    Value = ValueNode.InnerText;
                    Code = XmlHelper.Attribute(ValueNode, "code");
                }
                if (Value == "")
                    Value = MathUtility.MissingValue.ToString();
                
                AllValues.Add(Convert.ToDouble(Value));
                if (ValueNode != null &&
                    XmlHelper.Attribute(ValueNode, "units") != "")
                    Units = XmlHelper.Attribute(ValueNode, "units");
            }
            Values = AllValues.ToArray();
        }

        /// <summary>
        /// Return the PAWC (DUL - CropLL) for the specified soil.
        /// </summary>
        [WebMethod]
        public double PAWC(string SoilName, string CropName)
        {
            Soil Soil = Soil.Create(SoilXML(SoilName));
            if (!StringManip.Contains(Soil.CropNames, CropName))
                CropName = "Wheat";
            double[] PAWCmm = MathUtility.Multiply(Soil.PAWCCrop(CropName), Soil.Thickness);
            return MathUtility.Sum(PAWCmm);
        }

        /// <summary>
        /// Convert the soil sample XML to the new format.
        /// </summary>
        [WebMethod]
        public string ConvertSoilSampleXML(string SoilSampleXML)
        {
            string OldSoilFormat = "<folder version=\"7\">" +
                                   "<soil name=\"test\">" +
                                   SoilSampleXML +
                                   "</soil>" +
                                   "</folder>";
            XmlDocument Doc = new XmlDocument();
            Doc.LoadXml(OldSoilFormat);

            ApsimFile.APSIMChangeTool.Upgrade(Doc.DocumentElement);
            return XmlHelper.FindByType(Doc.DocumentElement, "soil/sample").OuterXml;
        }

        /// <summary>
        /// Return the bytes of a soil chart in PNG format. Google Earth uses this.
        /// YIELDPROPHET uses this call.
        /// </summary>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Xml)]
        public byte[] SoilChartPNG(string SoilName)
        {
            return SoilChartPNGFromXML(SoilXML(SoilName));
        }

        /// <summary>
        /// Return the bytes of a soil chart in PNG format. Google Earth uses this.
        /// YIELDPROPHET uses this call.
        /// </summary>
        [WebMethod]
        public byte[] SoilChartWithSamplePNG(string SoilName, double[] Thickness, double[] SW, bool IsGravimetric)
        {
            Soil Soil = Soil.Create(SoilXML(SoilName));

            RemoveMissingValues(ref Thickness, ref SW);

            Soil.Samples.Add(new Sample() { Thickness = Thickness, SW = SW });
            if (IsGravimetric)
                Soil.Samples[0].SWUnits = Sample.SWUnitsEnum.Gravimetric;

            SoilGraphUI Graph = CreateSoilGraph(Soil, true);

            // Take the LL15 line off the chart.
            foreach (Series S in Graph.Chart.Series)
            {
                if (S.Title == "LL15 (mm/mm)")
                    S.Active = false;
            }

            // Find the wheat CLL data column
            string WheatCLLColumnName = null;
            foreach (DataColumn Column in Graph.DataSources[0].Columns)
                if (Column.ColumnName.Contains("wheat LL") || Column.ColumnName.Contains("Wheat LL"))
                    WheatCLLColumnName = Column.ColumnName;

            // Add in the SW area.
            HorizArea SWSeries = new HorizArea();
            SWSeries.LinePen.Width = 2;
            SWSeries.Color = Color.FromArgb(0, 128, 192); // blue
            SWSeries.LinePen.Color = Color.FromArgb(0, 128, 192); // blue
            SWSeries.AreaLines.Visible = false;
            Graph.AddSeries(Graph.DataSources[0], "SW (mm/mm)", SWSeries);
            SWSeries.Active = true;
            Graph.Chart.Series.MoveTo(SWSeries, 2);

            if (WheatCLLColumnName != null)
            {
                //// Add in the Wheat CLL area.
                HorizArea CLL = new HorizArea();
                CLL.LinePen.Width = 2;
                CLL.Color = Color.White;
                CLL.LinePen.Color = Color.FromArgb(255, 128, 128); // pink
                CLL.AreaLines.Visible = false;
                Graph.AddSeries(Graph.DataSources[0], WheatCLLColumnName, CLL);
                CLL.Active = true;
                CLL.ShowInLegend = false;
                Graph.Chart.Series.MoveTo(CLL, 3);

                //// Add in the Wheat CLL line.
                Line CLLLine = new Line();
                CLLLine.LinePen.Width = 2;
                CLLLine.Color = Color.FromArgb(255, 128, 128);         // pink
                CLLLine.LinePen.Color = Color.FromArgb(255, 128, 128); // pink
                Graph.AddSeries(Graph.DataSources[0], WheatCLLColumnName, CLLLine);
                CLLLine.Active = true;
            }

            //// Add in the SW line.
            Line SWLine = new Line();
            SWLine.LinePen.Width = 2;
            SWLine.Color = Color.Blue;     // blue
            SWLine.LinePen.Color = Color.Blue; // blue
            Graph.AddSeries(Graph.DataSources[0], "SW (mm/mm)", SWLine);
            SWLine.Active = true;
            SWLine.ShowInLegend = false;

            Graph.PopulateSeries();
            Graph.Chart.Legend.CheckBoxes = false;

            MemoryStream MemStream = new MemoryStream(10000);
            Graph.Chart.Export.Image.PNG.Height = 550;
            Graph.Chart.Export.Image.PNG.Width = 550;
            Graph.Chart.Export.Image.PNG.Save(MemStream);

            return MemStream.ToArray();
        }

        /// <summary>
        /// Resize the Thickness and Values array to ensure they don't contain missing values.
        /// </summary>
        /// <param name="Thickness"></param>
        /// <param name="Values"></param>
        private void RemoveMissingValues(ref double[] Thickness, ref double[] Values)
        {
            if (Thickness.Length != Values.Length)
                return;
            for (int i = 0; i < Thickness.Length; i++)
            {
                if (Values[i] == double.NaN || Values[i] == MathUtility.MissingValue)
                {
                    if (i == 0) return;
                    Array.Resize(ref Thickness, i);
                    Array.Resize(ref Values, i);
                    return;
                }
            }
        }

        /// <summary>
        /// Return information about all soils that are within a given search radius of the specified lat and long.
        /// If SoilType is not null then only those soils that match the specified 
        /// SoilType are returned. The returned array is a list of soils that match the search criteria.
        /// Called by YieldProphet.
        /// </summary>
        [WebMethod]
        public SoilInfo[] SearchSoilsReturnInfo(double Latitude, double Longitude, double Radius, string SoilType)
        {
            List<SoilInfo> Soils = new List<SoilInfo>();
            SqlConnection Connection = Open();
            SqlDataReader Reader = null;
            try
            {
                XmlDocument Doc = new XmlDocument();

                SqlCommand Command = new SqlCommand("SELECT Name, XML FROM " + TableName, Connection);
                Reader = Command.ExecuteReader();
                while (Reader.Read())
                {
                    Doc.LoadXml(Reader["XML"].ToString());
                    double Lat, Long;
                    if (Double.TryParse(XmlHelper.Value(Doc.DocumentElement, "Latitude"), out Lat))
                    {
                        if (Double.TryParse(XmlHelper.Value(Doc.DocumentElement, "Longitude"), out Long))
                        {
                            double SoilDistance = distance(Latitude, Longitude, Lat, Long, 'K');
                            if (SoilDistance < Radius)
                            {
                                if (SoilType == null || SoilType.ToLower() == XmlHelper.Value(Doc.DocumentElement, "SoilType").ToLower())
                                {
                                    SoilInfo NewSoil = new SoilInfo();
                                    NewSoil.Name = Reader["Name"].ToString();
                                    NewSoil.Latitude = Convert.ToDouble(XmlHelper.Value(Doc.DocumentElement, "Latitude"));
                                    NewSoil.Longitude = Convert.ToDouble(XmlHelper.Value(Doc.DocumentElement, "Longitude"));
                                    NewSoil.SoilType = XmlHelper.Value(Doc.DocumentElement, "SoilType");
                                    NewSoil.Description = XmlHelper.Value(Doc.DocumentElement, "Description");
                                    NewSoil.Distance = SoilDistance;
                                    NewSoil.Site = XmlHelper.Value(Doc.DocumentElement, "Site");
                                    Soils.Add(NewSoil);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                Reader.Close();
                Connection.Close();
                throw err;
            }
            Reader.Close();
            Connection.Close();

            // Sort the soils and return them.
            Soils.Sort(CompareSoilLocations);
            return Soils.ToArray();
        }

        #region iPad app methods

        public class JsonSoilParam
        {
            public string JSonSoil;
        }
        public class PAWCJsonParams
        {
            public string JSonSoil;
        }
        public class PAWCByCrop
        {
            public string CropName;
            public double[] PAWC;
            public double PAWCTotal;
        }
        public class SoilInfo
        {
            public string Name;
            public string Description;
            public string SoilType;
            public double Latitude;
            public double Longitude;
            public double Distance;
            public string ASCOrder;
            public string ASCSubOrder;
            public string Site;
            public string Region;
            public string NearestTown;
        }
        public class SoilBasicInfo
        {
            public string Name;
            public double Latitude;
            public double Longitude;
        }
        public class SearchSoilsParams
        {
            public double Latitude;
            public double Longitude;
            public double Radius;
            public string ASCOrder;
            public string ASCSubOrder;
        }
        public class SoilAnalysisInfo
        {
            public string Name;
            public double[] Thickness;
            public string[] Texture;
            public double[] EC;
            public double[] PH;
            public double[] CL;
            public double[] Boron;
            public double[] ESP;
            public double[] AL;
            public double[] WheatLL;
            public double[] WheatXF;
            public double[] LL15;
            public double[] DUL;
        }
        /// <summary>
        /// Return the soil as a JSON string. Called from iPAD app.
        /// </summary>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Xml)]
        public string SoilAsJson(string Name)
        {
            // This method is marked as ResponseFormat.Xml to stop .NET from serialising the return string.
            // The return string is already in JSON format so no need for .NET to do it as well.
            XmlNode OldNode = ConvertSoilToOldFormat(SoilXML(Name));
            return JsonConvert.SerializeXmlNode(OldNode);
        }

        /// <summary>
        /// Update all soils to the specified .soils content. Called from iPAD soil app.
        /// </summary>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public bool UpdateUserSoil(JsonSoilParam Params)
        {
            SqlConnection Connection = Open();
            try
            {
                // Load in the XML
                XmlDocument SoilDoc = JsonConvert.DeserializeXmlNode(Params.JSonSoil);

                string Name = "/UserSoils/" + XmlHelper.Name(SoilDoc.DocumentElement);

                // Delete the existing soil if it exists
                SqlCommand Cmd = new SqlCommand("DELETE FROM " + TableName + " WHERE Name = @Name", Connection);
                Cmd.Parameters.Add(new SqlParameter("@Name", Name));
                Cmd.ExecuteNonQuery();

                // Add soil to DB
                AddSoil(Connection, Name, SoilDoc.OuterXml, false);
            }
            catch (Exception)
            {
                Connection.Close();
                return false;
            }
            Connection.Close();
            return true;
        }

        /// <summary>
        /// Return the PAWC (DUL - CropLL) for the specified soil. Called from iPAD app
        /// </summary>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public PAWCByCrop[] PAWCJson(PAWCJsonParams Params)
        {
            // Load in the soil XML
            XmlDocument SoilDoc = JsonConvert.DeserializeXmlNode(Params.JSonSoil);

            // Convert from old XML format to new.
            string NewXML = ConvertOldXmlToNew(SoilDoc.DocumentElement.OuterXml);

            Soil Soil = Soil.Create(NewXML);

            List<PAWCByCrop> PAWCs = new List<PAWCByCrop>();
            foreach (string CropName in Soil.CropNames)
            {
                if (Array.IndexOf(Soil.PredictedCropNames, CropName) == -1)
                {
                    double[] PAWCmm = MathUtility.Multiply(Soil.PAWCCrop(CropName), Soil.Thickness);
                    PAWCs.Add(new PAWCByCrop() { CropName = CropName, 
                                                 PAWC = PAWCmm,
                                                 PAWCTotal = MathUtility.Sum(PAWCmm) 
                                               });
                }
            }
            return PAWCs.ToArray();
        }

        /// <summary>
        /// Return the names of all soils that are within a given search radius of the specified lat and long.
        /// If ASC (Australian Soil Classification) is not null then only those soils that match the specified 
        /// ASC are returned. The returned string is a list of soils in JSON format:
        ///    e.g. [soilname1,soilname2]
        /// Called by Soil iPAD app.
        /// </summary>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public SoilInfo[] SearchSoils(SearchSoilsParams Params)
        {
            //StreamWriter Out3 = new StreamWriter("D:\\Websites\\FILES\\Transfer\\ApsoilWeb.txt", true);
            //Out3.WriteLine(DateTime.Now.ToString());
            //Out3.WriteLine("SearchSoils");
            //Out3.Close();


            List<SoilInfo> Soils = new List<SoilInfo>();
            SqlConnection Connection = Open();
            SqlDataReader Reader = null;
            try
            {
                XmlDocument Doc = new XmlDocument();

                SqlCommand Command = new SqlCommand("SELECT Name, XML FROM " + TableName + " WHERE IsApsoil=1", Connection);
                Reader = Command.ExecuteReader();
                while (Reader.Read())
                {
                    Doc.LoadXml(Reader["XML"].ToString());
                    double Lat, Long;
                    if (Params.ASCOrder == null || Params.ASCOrder == "" || Params.ASCOrder == XmlHelper.Value(Doc.DocumentElement, "ASCOrder"))
                    {
                        if (Params.ASCSubOrder == null || Params.ASCSubOrder == "" || Params.ASCSubOrder == XmlHelper.Value(Doc.DocumentElement, "ASCSubOrder"))
                        {
                            if (Double.TryParse(XmlHelper.Value(Doc.DocumentElement, "Latitude"), out Lat))
                            {
                                if (Double.TryParse(XmlHelper.Value(Doc.DocumentElement, "Longitude"), out Long))
                                {
                                    double SoilDistance = distance(Params.Latitude, Params.Longitude, Lat, Long, 'K');

                                    if (SoilDistance < Params.Radius)
                                    {
                                        SoilInfo NewSoil = new SoilInfo();
                                        NewSoil.Name = Reader["Name"].ToString();
                                        NewSoil.Distance = SoilDistance;
                                        NewSoil.Description = XmlHelper.Value(Doc.DocumentElement, "SoilType");
                                        NewSoil.Latitude = Convert.ToDouble(XmlHelper.Value(Doc.DocumentElement, "Latitude"));
                                        NewSoil.Longitude = Convert.ToDouble(XmlHelper.Value(Doc.DocumentElement, "Longitude"));
                                        NewSoil.ASCOrder = XmlHelper.Value(Doc.DocumentElement, "ASCOrder");
                                        NewSoil.ASCSubOrder = XmlHelper.Value(Doc.DocumentElement, "ASCSubOrder");
                                        NewSoil.Site = XmlHelper.Value(Doc.DocumentElement, "Site");
                                        Soils.Add(NewSoil);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                Reader.Close();
                Connection.Close();

                StreamWriter out1 = new StreamWriter("D:\\Websites\\FILES\\Transfer\\ApsoilWeb.txt", true);
                out1.WriteLine("Error:" + err.Message);
                out1.Close();

                throw err;
            }
            Reader.Close();
            Connection.Close();

            // Sort the soils and return them.
            Soils.Sort(CompareSoilLocations);

            //StreamWriter Out = new StreamWriter("D:\\Websites\\FILES\\Transfer\\ApsoilWeb.txt", true);
            //Out.WriteLine("Number of soils returned:" + Soils.Count.ToString());
            //Out.Close();

            return Soils.ToArray();
        }

        /// <summary>
        /// Return the names, lats and longs of all australian soils
        /// Called by Soil iPAD app.
        /// </summary>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public SoilBasicInfo[] AllAustralianSoils(SearchSoilsParams Params)
        {
            //StreamWriter Out = new StreamWriter("D:\\Websites\\FILES\\Transfer\\ApsoilWeb.txt", true);
            //Out.WriteLine(DateTime.Now.ToString());
            //Out.WriteLine("AllAustralianSoils");
            //Out.Close();

            List<SoilBasicInfo> Soils = new List<SoilBasicInfo>();
            SqlConnection Connection = Open();
            SqlDataReader Reader = null;
            try
            {
                XmlDocument Doc = new XmlDocument();

                SqlCommand Command = new SqlCommand("SELECT Name, XML FROM " + TableName + " WHERE IsApsoil=1", Connection);
                Reader = Command.ExecuteReader();
                while (Reader.Read())
                {
                    Doc.LoadXml(Reader["XML"].ToString());

                    double Lat, Long;

                    if (Double.TryParse(XmlHelper.Value(Doc.DocumentElement, "Latitude"), out Lat) &&
                        Double.TryParse(XmlHelper.Value(Doc.DocumentElement, "Longitude"), out Long) &&
                        XmlHelper.Value(Doc.DocumentElement, "Country") == "Australia")
                    {
                        SoilBasicInfo NewSoil = new SoilBasicInfo();
                        NewSoil.Name = Reader["Name"].ToString();
                        NewSoil.Latitude = Convert.ToDouble(XmlHelper.Value(Doc.DocumentElement, "Latitude"));
                        NewSoil.Longitude = Convert.ToDouble(XmlHelper.Value(Doc.DocumentElement, "Longitude"));
                        if (!double.IsNaN(NewSoil.Latitude) && !double.IsNaN(NewSoil.Longitude))
                            Soils.Add(NewSoil);

                    }
                }
            }
            catch (Exception err)
            {
                Reader.Close();
                Connection.Close();

                StreamWriter Out2 = new StreamWriter("D:\\Websites\\FILES\\Transfer\\ApsoilWeb.txt", true);
                Out2.WriteLine("Error: " + err.Message);
                Out2.Close();

                throw err;
            }
            Reader.Close();
            Connection.Close();

            //StreamWriter Out3 = new StreamWriter("D:\\Websites\\FILES\\Transfer\\ApsoilWeb.txt", true);
            //Out3.WriteLine("Num soils returned: " + Soils.Count);
            //Out3.Close();
            return Soils.ToArray();
        }

        /// <summary>
        /// Return the bytes of a soil chart in PNG format. Google Earth uses this.
        /// iPad soil app uses this call. It is actually called via SoilChart.aspx rather
        /// than directly from the soil app - hence the XML argument.
        /// </summary>
        [WebMethod]
        public byte[] SoilChartPNGFromXML(string XML)
        {
            string NewXML = ConvertOldXmlToNew(XML);

            Soil Soil = Soil.Create(NewXML);

            SoilGraphUI Graph = CreateSoilGraph(Soil, false);

            //StreamWriter Out = new StreamWriter("D:\\Websites\\FILES\\Transfer\\ApsoilWeb.txt", true);
            //Out.WriteLine("xml: " + NewXML);
            //Out.Close();
            
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

        #endregion

        #region Privates

        /// <summary>
        /// Open the SoilsDB ready for use.
        /// </summary>
        private SqlConnection Open()
        {
            string ConnectionString = File.ReadAllText(@"\\IIS-EXT1\APSIM-Sites\dbConnect.txt") + ";Database=APSoil";

            SqlConnection Connection = new SqlConnection(ConnectionString);
            Connection.Open();
            return Connection;
        }

        /// <summary>
        /// Used once to convert all soils to new format.
        /// </summary>
        public void ConvertOldSoilsToNewSoils()
        {

            SqlConnection Connection = Open();
            SqlConnection Connection2 = Open();
            SqlDataReader Reader = null;
            try
            {
                SqlCommand Command;
                Command = new SqlCommand("DROP TABLE AllSoils", Connection);
                Command.ExecuteNonQuery();

                Command = new SqlCommand("SELECT * INTO AllSoils FROM Soils", Connection);
                Command.ExecuteNonQuery();

                Command = new SqlCommand("SELECT * FROM AllSoils", Connection);

                Reader = Command.ExecuteReader();
                while (Reader.Read())
                {
                    string Name = Reader["Name"].ToString();
                    string XML = Reader["XML"].ToString();
                    bool IsApsoil = Convert.ToBoolean(Reader["IsApsoil"]);

                    string NewXML = ConvertOldXmlToNew(XML);                    

                    string SQL = "UPDATE AllSoils SET XML = @XML, IsApsoil = @IsApsoil WHERE Name = @Name";
                    SqlCommand Cmd = new SqlCommand(SQL, Connection2);
                    Cmd.Parameters.Add(new SqlParameter("@Name", Name));
                    Cmd.Parameters.Add(new SqlParameter("@XML", NewXML));
                    Cmd.Parameters.Add(new SqlParameter("@IsApsoil", IsApsoil));
                    Cmd.ExecuteNonQuery();

                }
                Reader.Close();
            }
            catch (Exception err)
            {
                Reader.Close();
                Connection.Close();
                Connection2.Close();
                throw err;
            }
            Connection.Close();
            Connection2.Close();
        }

        private static string ConvertOldXmlToNew(string XML)
        {
            XmlDocument Doc = new XmlDocument();
            Doc.LoadXml("<dummy><folder>" + XML + "</folder></dummy>");
            XmlHelper.SetAttribute(Doc.DocumentElement, "version", "31");
            APSIMChangeTool.UpgradeToVersion(Doc.DocumentElement, 34);

            XmlNode soilNode = XmlHelper.FindByType(Doc.DocumentElement, "folder/Soil");
            if (soilNode == null)
                throw new Exception("Cannot find soil node while converting old soil xml to new soil xml");

            Soil soil = Soil.Create(soilNode.OuterXml);

            // Make sure it has a <SoilWater> node.
            if (soil.SoilWater == null)
            {
                soil.SoilWater = new SoilWater();
                soil.SoilWater.SummerCona = 3.5;
                soil.SoilWater.SummerU = 6;
                soil.SoilWater.SummerDate = "1-Nov";
                soil.SoilWater.WinterCona = 2;
                soil.SoilWater.WinterU = 2;
                soil.SoilWater.WinterDate = "1-Apr";
                soil.SoilWater.DiffusConst = 40;
                soil.SoilWater.DiffusSlope = 16;
                soil.SoilWater.Salb = 0.13;
                soil.SoilWater.CN2Bare = 73;
                soil.SoilWater.CNRed = 20;
                soil.SoilWater.CNCov = 0.8;
                soil.SoilWater.Thickness = soil.Thickness;
                soil.SoilWater.SWCON = new double[soil.Thickness.Length];
                for (int i = 0; i < soil.Thickness.Length; i++)
                    soil.SoilWater.SWCON[i] = 0.3;
                return soil.ToXml();
            }
            else
                return  XmlHelper.Find(Doc.DocumentElement, "folder").InnerXml;
        }


        /// <summary>
        /// Converts a new soil XML to the old format for the iPad app.
        /// Old format:
        ///    <soil name="Soil">
        ///       <Comment type="multiedit" description="Comments" />
        ///       <ASC_Order description="Australian Soil Classification Order" />
        ///       <ASC_Sub-order description="Australian Soil Classification Sub-Order" />
        ///       <SoilType description="Soil description">Black Vertosol</SoilType>
        ///       <LocalName>Waco</LocalName>
        ///       ...
        ///       <Water>
        ///         <Layer>
        ///           <Thickness units="mm">150</Thickness>
        ///           <KS units="mm/day" />
        ///           <BD units="g/cc">1.02</BD>
        ///           <AirDry units="mm/mm">0.15</AirDry>
        ///           <LL15 units="mm/mm">0.29</LL15>
        ///           <DUL units="mm/mm">0.54</DUL>
        ///           <SAT units="mm/mm">0.59</SAT>
        ///         </Layer>
        ///         ...
        /// </summary>
        private static XmlNode ConvertSoilToOldFormat(string NewXML)
        {
            Soil Soil = Soil.Create(NewXML);

            XmlDocument Doc = new XmlDocument();
            XmlNode SoilNode = Doc.AppendChild(Doc.CreateElement("soil"));
            XmlHelper.SetName(SoilNode, Soil.Name);
            SetValue(SoilNode, "ASC_Order", Soil.ASCOrder);
            SetValue(SoilNode, "ASC_Sub-order", Soil.ASCSubOrder);
            SetValue(SoilNode, "SoilType", Soil.SoilType);
            SetValue(SoilNode, "LocalName", Soil.LocalName);
            SetValue(SoilNode, "Site", Soil.Site, false);
            SetValue(SoilNode, "NearestTown", Soil.NearestTown);
            SetValue(SoilNode, "Region", Soil.Region, false);
            SetValue(SoilNode, "State", Soil.State, false);
            SetValue(SoilNode, "Country", Soil.Country, false);
            SetValue(SoilNode, "NaturalVegetation", Soil.NaturalVegetation);
            SetValue(SoilNode, "ApsoilNumber", Soil.ApsoilNumber);
            SetValue(SoilNode, "Latitude", Soil.Latitude.ToString());
            SetValue(SoilNode, "Longitude", Soil.Longitude.ToString());
            SetValue(SoilNode, "LocationAccuracy", Soil.LocationAccuracy);
            SetValue(SoilNode, "YearOfSampling", Soil.YearOfSampling.ToString());
            SetValue(SoilNode, "DataSource", Soil.DataSource);
            SetValue(SoilNode, "Comments", Soil.Comments);

            XmlNode WaterNode = SoilNode.AppendChild(Doc.CreateElement("Water"));
            WriteLayeredData(WaterNode, "Thickness", "mm", null, Soil.Thickness, Soil.Thickness.Length);
            WriteLayeredData(WaterNode, "KS", "mm/day", Soil.Water.KSMetadata, Soil.Water.KS, Soil.Thickness.Length);
            WriteLayeredData(WaterNode, "BD", "g/cc", Soil.Water.BDMetadata, Soil.Water.BD, Soil.Thickness.Length);
            WriteLayeredData(WaterNode, "Airdry", "mm/mm", Soil.Water.AirDryMetadata, Soil.AirDry, Soil.Thickness.Length);
            WriteLayeredData(WaterNode, "LL15", "mm/mm", Soil.Water.LL15Metadata, Soil.LL15, Soil.Thickness.Length);
            WriteLayeredData(WaterNode, "DUL", "mm/mm", Soil.Water.DULMetadata, Soil.DUL, Soil.Thickness.Length);
            WriteLayeredData(WaterNode, "SAT", "mm/mm", Soil.Water.SATMetadata, Soil.SAT, Soil.Thickness.Length);

            foreach (string CropName in Soil.CropNames)
            {
                XmlNode CropNode = WaterNode.AppendChild(Doc.CreateElement("SoilCrop"));
                XmlHelper.SetName(CropNode, CropName);
                WriteLayeredData(CropNode, "Thickness", "mm", null, Soil.Crop(CropName).Thickness, Soil.Thickness.Length);
                WriteLayeredData(CropNode, "ll", "mm/mm", Soil.Crop(CropName).LLMetadata, Soil.Crop(CropName).LL, Soil.Thickness.Length);
                WriteLayeredData(CropNode, "kl", "/day", null, Soil.Crop(CropName).KL, Soil.Thickness.Length);
                WriteLayeredData(CropNode, "xf", "0-1", null, Soil.Crop(CropName).XF, Soil.Thickness.Length);
            }

            XmlNode SoilOrganicMatter = SoilNode.AppendChild(Doc.CreateElement("SoilOrganicMatter"));
            SetValue(SoilOrganicMatter, "RootCn", Soil.SoilOrganicMatter.RootCN.ToString(), false);
            SetValue(SoilOrganicMatter, "RootWt", Soil.SoilOrganicMatter.RootWt.ToString(), false);
            SetValue(SoilOrganicMatter, "SoilCn", Soil.SoilOrganicMatter.SoilCN.ToString(), false);
            SetValue(SoilOrganicMatter, "EnrACoeff", Soil.SoilOrganicMatter.EnrACoeff.ToString(), false);
            SetValue(SoilOrganicMatter, "EnrBCoeff", Soil.SoilOrganicMatter.EnrBCoeff.ToString(), false);
            WriteLayeredData(SoilOrganicMatter, "Thickness", "mm", null, Soil.SoilOrganicMatter.Thickness, Soil.SoilOrganicMatter.Thickness.Length);
            WriteLayeredData(SoilOrganicMatter, "OC", Soil.SoilOrganicMatter.OCUnits.ToString(), Soil.SoilOrganicMatter.OCMetadata, Soil.SoilOrganicMatter.OC, Soil.SoilOrganicMatter.Thickness.Length);
            WriteLayeredData(SoilOrganicMatter, "FBIOM", "0-1", null, Soil.SoilOrganicMatter.FBiom, Soil.SoilOrganicMatter.Thickness.Length);
            WriteLayeredData(SoilOrganicMatter, "FINERT", "0-1", null, Soil.SoilOrganicMatter.FInert, Soil.SoilOrganicMatter.Thickness.Length);

            XmlNode Analysis = SoilNode.AppendChild(Doc.CreateElement("Analysis"));
            WriteLayeredData(Analysis, "Thickness", "mm", null, Soil.Analysis.Thickness, Soil.Analysis.Thickness.Length);
            WriteLayeredData(Analysis, "Rocks", "%", null, Soil.Analysis.Rocks, Soil.Analysis.Thickness.Length);
            WriteLayeredData(Analysis, "Texture", "", null, Soil.Analysis.Texture, Soil.Analysis.Thickness.Length);
            WriteLayeredData(Analysis, "MunsellColour", "", null, Soil.Analysis.MunsellColour, Soil.Analysis.Thickness.Length);
            WriteLayeredData(Analysis, "EC", "1:5 dS/m", Soil.Analysis.ECMetadata, Soil.Analysis.EC, Soil.Analysis.Thickness.Length);
            WriteLayeredData(Analysis, "PH", "1:5 water", Soil.Analysis.PHMetadata, Soil.Analysis.PH, Soil.Analysis.Thickness.Length);
            WriteLayeredData(Analysis, "CL", "mg/kg", Soil.Analysis.CLMetadata, Soil.Analysis.CL, Soil.Analysis.Thickness.Length);
            WriteLayeredData(Analysis, "Boron", Soil.Analysis.BoronUnits.ToString(), Soil.Analysis.BoronMetadata, Soil.Analysis.Boron, Soil.Analysis.Thickness.Length);
            WriteLayeredData(Analysis, "CEC", "cmol+/kg", Soil.Analysis.CECMetadata, Soil.Analysis.CEC, Soil.Analysis.Thickness.Length);
            WriteLayeredData(Analysis, "Ca", "cmol+/kg", Soil.Analysis.CaMetadata, Soil.Analysis.Ca, Soil.Analysis.Thickness.Length);
            WriteLayeredData(Analysis, "Mg", "cmol+/kg", Soil.Analysis.MgMetadata, Soil.Analysis.Mg, Soil.Analysis.Thickness.Length);
            WriteLayeredData(Analysis, "Na", "cmol+/kg", Soil.Analysis.NaMetadata, Soil.Analysis.Na, Soil.Analysis.Thickness.Length);
            WriteLayeredData(Analysis, "K", "cmol+/kg", Soil.Analysis.KMetadata, Soil.Analysis.K, Soil.Analysis.Thickness.Length);
            WriteLayeredData(Analysis, "ESP", "%", Soil.Analysis.ESPMetadata, Soil.Analysis.ESP, Soil.Analysis.Thickness.Length);
            WriteLayeredData(Analysis, "Mn", "mg/kg", Soil.Analysis.MnMetadata, Soil.Analysis.Mn, Soil.Analysis.Thickness.Length);
            WriteLayeredData(Analysis, "Al", "cmol+/kg", Soil.Analysis.AlMetadata, Soil.Analysis.Al, Soil.Analysis.Thickness.Length);
            WriteLayeredData(Analysis, "ParticleSizeSand", "%", Soil.Analysis.ParticleSizeSandMetadata, Soil.Analysis.ParticleSizeSand, Soil.Analysis.Thickness.Length);
            WriteLayeredData(Analysis, "ParticleSizeSilt", "%", Soil.Analysis.ParticleSizeSiltMetadata, Soil.Analysis.ParticleSizeSilt, Soil.Analysis.Thickness.Length);
            WriteLayeredData(Analysis, "ParticleSizeClay", "%", Soil.Analysis.ParticleSizeClayMetadata, Soil.Analysis.ParticleSizeClay, Soil.Analysis.Thickness.Length);

            return Doc.DocumentElement;
        }

        /// <summary>
        /// Write old style parameter to the specified SoilNode. e.g.
        ///     <ASC_Order description="Australian Soil Classification Order" />       
        /// </summary>
        private static void SetValue(XmlNode SoilNode, string Name, string Value, bool AddDescription=true)
        {
            XmlNode ValueNode = SoilNode.AppendChild(SoilNode.OwnerDocument.CreateElement(Name));
            if (AddDescription)
                XmlHelper.SetAttribute(ValueNode, "description", Name);
            if (Value != "" && Value != null)
                ValueNode.InnerText = Value;
        }

        /// <summary>
        /// Write old style layered data to the specified node. e.g.
        /// <Layer>
        ///    <Thickness units="mm">150</Thickness>
        ///    <KS units="mm/day" />
        ///    <BD units="g/cc">1.02</BD>
        ///    <AirDry units="mm/mm">0.15</AirDry>
        ///    <LL15 units="mm/mm">0.29</LL15>
        ///    <DUL units="mm/mm">0.54</DUL>
        ///    <SAT units="mm/mm">0.59</SAT>
        ///  </Layer>
        /// </summary>
        private static void WriteLayeredData(XmlNode Node, string VariableName, string Units, string[] Codes, double[] Values, int NumValuesToWrite)
        {
            string[] StringValues = null;
            if (Values != null)
                StringValues = MathUtility.DoublesToStrings(Values);
            WriteLayeredData(Node, VariableName, Units, Codes, StringValues, NumValuesToWrite);
        }
        private static void WriteLayeredData(XmlNode Node, string VariableName, string Units, string[] Codes, string[] Values, int NumValuesToWrite)
        {
            XmlHelper.EnsureNumberOfChildren(Node, "Layer", "Layer", NumValuesToWrite);
            List<XmlNode> Layers = XmlHelper.ChildNodes(Node, "Layer");
            for (int i = 0; i < Layers.Count; i++)
            {
                XmlHelper.DeleteAttribute(Layers[i], "name");

                // Get value.
                string Value = "";
                if (Values != null && i < Values.Length)
                    Value = Values[i].ToString();

                //Get units
                string UnitsToWrite = "";
                if (i == 0 && Units != "")
                    UnitsToWrite = Units;

                // Get code
                string CodeToWrite = "";
                if (Codes != null && i < Codes.Length)
                    CodeToWrite = Codes[i];

                // Create a node.
                XmlNode ValueNode = XmlHelper.EnsureNodeExists(Layers[i], VariableName);

                // Write units and code
                if (UnitsToWrite != "")
                    XmlHelper.SetAttribute(ValueNode, "units", UnitsToWrite);
                if (CodeToWrite != "")
                    XmlHelper.SetAttribute(ValueNode, "code", CodeToWrite);

                // Write value
                bool SomethingHasBeenWritten = UnitsToWrite != "" || CodeToWrite != "";
                if (Value != "")
                    XmlHelper.SetValue(Layers[i], VariableName, Value);
                
            }
        }

        /// <summary>
        /// Create and return a soil graph
        /// </summary>
        private static SoilGraphUI CreateSoilGraph(Soil Soil, bool WithSW)
        {
            SoilGraphUI Graph = new SoilGraphUI();
            Graph.Populate(Soil, "Water", WithSW);
            //Graph.OnRefresh();
            return Graph;
        }

        /// <summary>
        /// Recursively insert all soils into database.
        /// </summary>
        private void InsertFolderIntoDB(SqlConnection Connection, XmlNode FolderNode)
        {
            foreach (XmlNode SoilNode in XmlHelper.ChildNodes(FolderNode, "Soil"))
                AddSoil(Connection, XmlHelper.FullPath(SoilNode), SoilNode.OuterXml, true);

            foreach (XmlNode ChildFolderNode in XmlHelper.ChildNodes(FolderNode, "Folder"))
                InsertFolderIntoDB(Connection, ChildFolderNode);
        }

        /// <summary>
        /// Add a soil to the DB, updating the existing one if necessary.
        /// </summary>
        private void AddSoil(SqlConnection Connection, string Name, string XML, bool IsApsoil)
        {
            if (Name[Name.Length - 1] == '/')
                Name = Name.Remove(Name.Length - 1);

            if (Name[0] == '/')
                Name = Name.Remove(0, 1);

            string SQL;
            if (SoilNames().Contains(Name))
                SQL = "UPDATE " + TableName + " SET XML = @XML, IsApsoil = @IsApsoil WHERE Name = @Name";
            else
                SQL = "INSERT INTO " + TableName + " (Name, XML, IsApsoil) VALUES (@Name, @XML, @IsApsoil)";

            SqlCommand Cmd = new SqlCommand(SQL, Connection);
            Cmd.Parameters.Add(new SqlParameter("@Name", Name));
            Cmd.Parameters.Add(new SqlParameter("@XML", XML));
            Cmd.Parameters.Add(new SqlParameter("@IsApsoil", IsApsoil));
            Cmd.ExecuteNonQuery();
        }

        private static int CompareSoilLocations(SoilInfo x, SoilInfo y)
        {
            if (x == null || y == null)
                return 0;
            if (x.Description != null && x.Description.ToLower().Contains("generic"))
            {
                if (y.Description != null && !y.Description.ToLower().Contains("generic"))
                    return 1;  // X is generic but Y is NOT generic
            }
            else
            {
                if (y.Description != null && y.Description.ToLower().Contains("generic"))
                    return -1;  // X is NOT generic but T is generic
            }
            if (x.Distance == y.Distance)
                return 0;
            else if (x.Distance < y.Distance)
                return -1;
            else
                return 1;
        }

        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        //:::                                                                         :::
        //:::  This routine calculates the distance between two points (given the     :::
        //:::  latitude/longitude of those points). It is being used to calculate     :::
        //:::  the distance between two ZIP Codes or Postal Codes using our           :::
        //:::  ZIPCodeWorld(TM) and PostalCodeWorld(TM) products.                     :::
        //:::                                                                         :::
        //:::  Definitions:                                                           :::
        //:::    South latitudes are negative, east longitudes are positive           :::
        //:::                                                                         :::
        //:::  Passed to function:                                                    :::
        //:::    lat1, lon1 = Latitude and Longitude of point 1 (in decimal degrees)  :::
        //:::    lat2, lon2 = Latitude and Longitude of point 2 (in decimal degrees)  :::
        //:::    unit = the unit you desire for results                               :::
        //:::           where: 'M' is statute miles                                   :::
        //:::                  'K' is kilometers (default)                            :::
        //:::                  'N' is nautical miles                                  :::
        //:::                                                                         :::
        //:::  United States ZIP Code/ Canadian Postal Code databases with latitude   :::
        //:::  & longitude are available at http://www.zipcodeworld.com               :::
        //:::                                                                         :::
        //:::  For enquiries, please contact sales@zipcodeworld.com                   :::
        //:::                                                                         :::
        //:::  Official Web site: http://www.zipcodeworld.com                         :::
        //:::                                                                         :::
        //:::  Hexa Software Development Center © All Rights Reserved 2004            :::
        //:::                                                                         :::
        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        private static double distance(double lat1, double lon1, double lat2, double lon2, char unit)
        {
            double theta = lon1 - lon2;
            double dist = Math.Sin(deg2rad(lat1)) * Math.Sin(deg2rad(lat2)) + Math.Cos(deg2rad(lat1)) * Math.Cos(deg2rad(lat2)) * Math.Cos(deg2rad(theta));
            dist = Math.Acos(dist);
            dist = rad2deg(dist);
            dist = dist * 60 * 1.1515;
            if (unit == 'K')
            {
                dist = dist * 1.609344;
            }
            else if (unit == 'N')
            {
                dist = dist * 0.8684;
            }
            return (dist);
        }

        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        //::  This function converts decimal degrees to radians             :::
        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        private static double deg2rad(double deg)
        {
            return (deg * Math.PI / 180.0);
        }

        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        //::  This function converts radians to decimal degrees             :::
        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        private static double rad2deg(double rad)
        {
            return (rad / Math.PI * 180.0);
        }

        #endregion

    }
}
