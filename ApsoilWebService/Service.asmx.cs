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
        //public Service()
        //{
        //    Open();
        //}

        //protected override void Dispose(bool disposing)
        //{
        //    base.Dispose(disposing);
        //    Close();
        //}

        /// <summary>
        /// Open the SoilsDB ready for use.
        /// </summary>
        private SqlConnection Open()
        {
            // Create a connection to the database.

            // The first string is the debug version to run from Dean's computer.
            //string ConnectionString = "Server=www.apsim.info\\SQLEXPRESS;Database=APSoil;Trusted_Connection=True;";
            string ConnectionString = "Server=www.apsim.info\\SQLEXPRESS;Database=APSoil;Trusted_Connection=False;User ID=sv-login-internal;password=P@ssword123";

            SqlConnection Connection = new SqlConnection(ConnectionString);
            Connection.Open();
            return Connection;
        }

        /// <summary>
        /// Get a list of names from table. YieldProphet calls this.
        /// </summary>
        [WebMethod]
        public List<string> SoilNames()
        {
            SqlConnection Connection  = Open();
            List<string> _SoilNames = new List<string>();
            SqlDataReader Reader = null;
            try
            {
                SqlCommand Command = new SqlCommand("SELECT Name FROM Soils", Connection);
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
                    Command = new SqlCommand("SELECT Name FROM Soils", Connection);
                else
                    Command = new SqlCommand("SELECT Name FROM Soils WHERE IsApsoil=1", Connection);

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
            SqlCommand Cmd = new SqlCommand("DELETE FROM Soils WHERE IsApsoil = 1", Connection);
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

        public class JsonSoilParam
        {
            public string JSonSoil;
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
                SqlCommand Cmd = new SqlCommand("DELETE FROM Soils WHERE Name = @Name", Connection);
                Cmd.Parameters.Add(new SqlParameter("@Name", Name));
                Cmd.ExecuteNonQuery();

                // Add soil to DB
                AddSoil(Connection, Name, SoilDoc.OuterXml, false);
            }
            catch (Exception err)
            {
                Connection.Close();
                return false;
            }
            Connection.Close();
            return true;
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

            string SQL;
            if (SoilNames().Contains(Name))
                SQL = "UPDATE Soils SET XML = @XML, IsApsoil = @IsApsoil WHERE Name = @Name";
            else
                SQL = "INSERT INTO Soils (Name, XML, IsApsoil) VALUES (@Name, @XML, @IsApsoil)";

            SqlCommand Cmd = new SqlCommand(SQL, Connection);
            Cmd.Parameters.Add(new SqlParameter("@Name", Name));
            Cmd.Parameters.Add(new SqlParameter("@XML", XML));
            Cmd.Parameters.Add(new SqlParameter("@IsApsoil", IsApsoil));
            Cmd.ExecuteNonQuery();
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
                SqlCommand Command = new SqlCommand("SELECT XML FROM Soils WHERE Name = @Name", Connection);
                Command.Parameters.Add(new SqlParameter("@Name", Name));
                Reader = Command.ExecuteReader();
                if (Reader.Read())
                {
                    XmlDocument Doc = new XmlDocument();
                    Doc.LoadXml(Reader["XML"].ToString());
                    ReturnString = Doc.OuterXml;
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
            return ReturnString;
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
                Doc.LoadXml(SoilXML(SoilName));
                string SoilType = Soil.Get(Doc.DocumentElement, "SoilType").Value;
                if (SoilType != "" && SoilType != null)
                    Types.Add(SoilType);
            }
            return Types.ToArray();
        }

        /// <summary>
        /// Return the soil as a JSON string. Called from iPAD app.
        /// </summary>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Xml)] 
        public string SoilAsJson(string Name)
        {
            SqlConnection Connection = Open();
            SqlDataReader Reader = null;
            string ReturnJSon = "";
            try
            {
                // This method is marked as ResponseFormat.Xml to stop .NET from serialising the return string.
                // The return string is already in JSON format so no need for .NET to do it as well.
                SqlCommand Command = new SqlCommand("SELECT XML FROM Soils WHERE Name = @Name", Connection);
                Command.Parameters.Add(new SqlParameter("@Name", Name));
                Reader = Command.ExecuteReader();
                if (Reader.Read())
                {
                    XmlDocument Doc = new XmlDocument();
                    Doc.LoadXml(Reader["XML"].ToString());
                    ReturnJSon = JsonConvert.SerializeXmlNode(Doc.DocumentElement);
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
            return ReturnJSon;
        }

        [WebMethod]
        public string SoilXMLAll()
        {
            StringBuilder XML = new StringBuilder(100000);

            foreach (string SoilName in SoilNames())
                XML.Append(SoilXML(SoilName));

            // The XML to this point is a flat list of <soil> nodes. We need to create <folder> noes
            // with hierarchy.
            XmlDocument Doc = new XmlDocument();
            Doc.LoadXml("<folder>" + XML.ToString() + "</folder>");
            DataTable Data = SoilDataTable.XMLToTable(Doc.DocumentElement, null);
            XmlNode NewXML = SoilDataTable.TableToXML(Data, null);
            return NewXML.OuterXml;
        }

        /// <summary>
        /// Return the PAW (SW - CropLL) for the specified soil.
        /// </summary>
        [WebMethod]
        public double PAW(string SoilName, string SoilSampleXML, string CropName)
        {
            //StreamWriter Out = new StreamWriter("D:\\Websites\\FILES\\Transfer\\ApsoilWeb.txt", true);
            //Out.WriteLine(DateTime.Now.ToString());
            //Out.WriteLine(SoilSampleXML);
            //Out.WriteLine();
            //Out.Close();

            // Load in the soil XML
            XmlDocument SoilDoc = new XmlDocument();
            SoilDoc.LoadXml(SoilXML(SoilName));

            // Load the sample XML into the Soil XML
            XmlDocument SampleDoc = new XmlDocument();
            SampleDoc.LoadXml(SoilSampleXML);
            SoilDoc.DocumentElement.AppendChild(SoilDoc.ImportNode(SampleDoc.DocumentElement, true));

            // Return the PAW in mm
            Soil.Variable PAW = Soil.Get(SoilDoc.DocumentElement, CropName + " PAW");
            PAW.Units = "mm";
            return MathUtility.Sum(PAW.Doubles);
        }

        /// <summary>
        /// Return the PAWC (DUL - CropLL) for the specified soil.
        /// </summary>
        [WebMethod]
        public double PAWC(string SoilName, string CropName)
        {
            // Load in the soil XML
            XmlDocument SoilDoc = new XmlDocument();
            SoilDoc.LoadXml(SoilXML(SoilName));

            // Return the PAWC in mm
            Soil.Variable PAWC = Soil.Get(SoilDoc.DocumentElement, CropName + " PAWC");
            PAWC.Units = "mm";
            return MathUtility.Sum(PAWC.Doubles);
        }


        public class PAWCJsonParams
        {
            public string JSonSoil;
        }


        public class PAWCByCrop
        {
            public string CropName;
            public double[] PAWC;
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

            List<PAWCByCrop> PAWCs = new List<PAWCByCrop>();
            foreach (string CropName in Soil.CropsMeasured(SoilDoc.DocumentElement))
            {
                Soil.Variable PAWC = Soil.Get(SoilDoc.DocumentElement, CropName + " PAWC");
                PAWC.Units = "mm";
                PAWCs.Add(new PAWCByCrop() {CropName = CropName, PAWC = PAWC.Doubles});
            }
            return PAWCs.ToArray();
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
        /// Create soil sample 1 XML
        /// </summary>
        [WebMethod]
        public string CreateSoilSample1XML(DateTime SampleDate, string[] DepthStrings, string SWUnits, double[] SW, double[] NO3, double[] NH4)
        {
            string SoilXML = "<soil name=\"test\">" +
                             "<Sample name=\"Soil sample 1\">" +
                                 "<Date type=\"date\" description=\"Sample date:\" />" +
                                 "<Layer>" +
                                 "   <Thickness units=\"mm\"></Thickness>" +
                                 "   <NO3 units=\"ppm\"></NO3>" +
                                 "   <NH4 units=\"ppm\"></NH4>" +
                                 "   <SW units=\"mm/mm\"></SW>" +
                                 "</Layer>" +
                              "</Sample>" +
                              "</soil>";
            XmlDocument Doc = new XmlDocument();
            Doc.LoadXml(SoilXML);

            double[] ThicknessMM = SoilUtility.ToThickness(DepthStrings);
            ThicknessMM = MathUtility.Multiply_Value(ThicknessMM, 10);
            Soil.Set(Doc.DocumentElement, new Soil.Variable("SW", SWUnits, SW, ThicknessMM, Doc.DocumentElement));
            Soil.Set(Doc.DocumentElement, new Soil.Variable("NO3", "ppm", NO3, ThicknessMM, Doc.DocumentElement));
            Soil.Set(Doc.DocumentElement, new Soil.Variable("NH4", "ppm", NH4, ThicknessMM, Doc.DocumentElement));

            // Set the sample date.
            XmlNode SampleNode = XmlHelper.Find(Doc.DocumentElement, "Soil sample 1");
            XmlHelper.SetValue(SampleNode, "Date", SampleDate.ToString("dd/MM/yyyy"));
            return SampleNode.OuterXml;
        }

        /// <summary>
        /// Create soil sample 2 XML
        /// </summary>
        [WebMethod]
        public string CreateSoilSample2XML(DateTime SampleDate, string[] DepthStrings, double[] OC, double[] EC, double[] PH, double[] CL)
        {
            string SoilXML = "<soil name=\"test\">" +
                             "<Sample name=\"Soil sample 2\">" +
                                 "<Date type=\"date\" description=\"Sample date:\" />" +
                                 "<Layer>" +
                                 "   <Thickness units=\"mm\"></Thickness>" +
                                 //"   <OC units=\"Walkley Black %\"></OC>" +
                                 //"   <EC units=\"1:5 dS/m\"></EC>" +
                                 //"   <PH units=\"1:5 water\"></PH>" +
                                 //"   <CL units=\"mg/kg\"></CL>" +
                                 "</Layer>" +
                              "</Sample>" +
                              "</soil>";
            XmlDocument Doc = new XmlDocument();
            Doc.LoadXml(SoilXML);

            double[] ThicknessMM = SoilUtility.ToThickness(DepthStrings);
            ThicknessMM = MathUtility.Multiply_Value(ThicknessMM, 10);
            if (MathUtility.ValuesInArray(OC))
            {
                XmlNode Node = XmlHelper.EnsureNodeExists(Doc.DocumentElement, "Soil sample 2/Layer/OC");
                //XmlHelper.SetAttribute(Node, "units", "Walkley Black %");
                Soil.Set(Doc.DocumentElement, new Soil.Variable("OC", "Walkley Black %", OC, ThicknessMM, Doc.DocumentElement));
            }

            if (MathUtility.ValuesInArray(EC))
            {
                XmlNode Node = XmlHelper.EnsureNodeExists(Doc.DocumentElement, "Soil sample 2/Layer/EC");
                //XmlHelper.SetAttribute(Node, "units", "1:5 dS/m");
                Soil.Set(Doc.DocumentElement, new Soil.Variable("EC", "1:5 dS/m", EC, ThicknessMM, Doc.DocumentElement));
            }

            if (MathUtility.ValuesInArray(PH))
            {
                XmlNode Node = XmlHelper.EnsureNodeExists(Doc.DocumentElement, "Soil sample 2/Layer/PH");
                //XmlHelper.SetAttribute(Node, "units", "1:5 water");
                Soil.Set(Doc.DocumentElement, new Soil.Variable("PH", "1:5 water", PH, ThicknessMM, Doc.DocumentElement));
            }

            if (MathUtility.ValuesInArray(CL))
            {
                XmlNode Node = XmlHelper.EnsureNodeExists(Doc.DocumentElement, "Soil sample 2/Layer/CL");
                //XmlHelper.SetAttribute(Node, "units", "mg/kg");
                Soil.Set(Doc.DocumentElement, new Soil.Variable("CL", "mg/kg", CL, ThicknessMM, Doc.DocumentElement));
            }
            // Set the sample date.
            XmlNode SampleNode = XmlHelper.Find(Doc.DocumentElement, "Soil sample 2");
            XmlHelper.SetValue(SampleNode, "Date", SampleDate.ToString("dd/MM/yyyy"));
            return SampleNode.OuterXml;
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
        /// iPad soil app uses this call.
        /// </summary>
        [WebMethod]
        public byte[] SoilChartPNGFromXML(string XML)
        {
            XmlDocument Doc = new XmlDocument();
            Doc.LoadXml(XML);


            XmlNode SoilNode = Doc.DocumentElement;
            XmlNode WaterNode = XmlHelper.Find(SoilNode, "Water");

            SoilGraphUI Graph = CreateSoilGraph(SoilNode, WaterNode, false);

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

            Context.Response.ContentType = "image/png";
            return MemStream.ToArray();
        }

        /// <summary>
        /// Create and return a soil graph
        /// </summary>
        private static SoilGraphUI CreateSoilGraph(XmlNode SoilNode, XmlNode WaterNode, bool WithSW)
        {
            DataTable Table = new DataTable();
            Table.TableName = "Water";
            List<string> VariableNames = Soil.ValidVariablesForProfileNode(WaterNode);
            VariableNames.RemoveAt(0);
            VariableNames.Insert(0, "DepthMidPoints (mm)");

            if (WithSW)
                VariableNames.Add("SW (mm/mm)");

            foreach (string Crop in Soil.Crops(SoilNode))
                VariableNames.Add(Crop + " LL(mm/mm)");

            Soil.WriteToTable(SoilNode, Table, VariableNames);

            SoilGraphUI Graph = new SoilGraphUI();
            Graph.SoilNode = SoilNode;
            Graph.OnLoad(null, "", WaterNode.OuterXml);
            Graph.DataSources.Add(Table);
            Graph.OnRefresh();
            return Graph;
        }

        /// <summary>
        /// Return the bytes of a soil chart in PNG format. Google Earth uses this.
        /// YIELDPROPHET uses this call.
        /// </summary>
        [WebMethod]
        public byte[] SoilChartWithSamplePNG(string SoilName, string SoilSampleXML)
        {
            //StreamWriter Out = new StreamWriter("D:\\Websites\\FILES\\Transfer\\ApsoilWeb.txt", true);
            //Out.WriteLine(DateTime.Now.ToString());
            //Out.WriteLine(SoilSampleXML);
            //Out.WriteLine();
            //Out.Close();

            XmlDocument Doc = new XmlDocument();
            Doc.LoadXml(SoilXML(SoilName));
            XmlNode SoilNode = Doc.DocumentElement;
            XmlNode WaterNode = XmlHelper.Find(SoilNode, "Water");

            // Add in the soil sample.
            XmlDocument SampleDoc = new XmlDocument();
            SampleDoc.LoadXml(SoilSampleXML);
            Doc.DocumentElement.AppendChild(Doc.ImportNode(SampleDoc.DocumentElement, true));

            SoilGraphUI Graph = CreateSoilGraph(SoilNode, WaterNode, true);

            // Take the LL15 line off the chart.
            foreach (Series S in Graph.Chart.Series)
            {
                if (S.Title == "LL15 (mm/mm)")
                    S.Active = false;
            }

            // Add in the SW area.
            HorizArea SW = new HorizArea();
            SW.LinePen.Width = 2;
            SW.Color = Color.FromArgb(0, 128, 192); // blue
            SW.LinePen.Color = Color.FromArgb(0, 128, 192); // blue
            SW.AreaLines.Visible = false;
            Graph.AddSeries(Graph.DataSources[0], "SW (mm/mm)", SW);
            SW.Active = true;
            Graph.Chart.Series.MoveTo(SW, 2);

            // Add in the Wheat CLL area.
            HorizArea CLL = new HorizArea();
            CLL.LinePen.Width = 2;
            CLL.Color = Color.White;
            CLL.LinePen.Color = Color.FromArgb(255, 128, 128); // pink
            CLL.AreaLines.Visible = false;
            Graph.AddSeries(Graph.DataSources[0], "wheat LL (mm/mm)", CLL);
            CLL.Active = true;
            CLL.ShowInLegend = false;
            Graph.Chart.Series.MoveTo(CLL, 3);

            // Add in the Wheat CLL line.
            Line CLLLine = new Line();
            CLLLine.LinePen.Width = 2;
            CLLLine.Color = Color.FromArgb(255, 128, 128);         // pink
            CLLLine.LinePen.Color = Color.FromArgb(255, 128, 128); // pink
            Graph.AddSeries(Graph.DataSources[0], "wheat LL (mm/mm)", CLLLine);
            CLLLine.Active = true;

            // Add in the SW line.
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
        }

        public class SoilBasicInfo
        {
            public string Name;
            public double Latitude;
            public double Longitude;
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

        public class SearchSoilsParams
        {
            public double Latitude;
            public double Longitude;
            public double Radius;
            public string ASCOrder;
            public string ASCSubOrder;
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
            List<SoilInfo> Soils = new List<SoilInfo>();
            SqlConnection Connection = Open();
            SqlDataReader Reader = null;
            try
            {
                XmlDocument Doc = new XmlDocument();

                SqlCommand Command = new SqlCommand("SELECT Name, XML FROM Soils WHERE IsApsoil=1", Connection);
                Reader = Command.ExecuteReader();
                while (Reader.Read())
                {
                    Doc.LoadXml(Reader["XML"].ToString());
                    double Lat, Long;
                    if (Params.ASCOrder == null || Params.ASCOrder == "" || Params.ASCOrder == XmlHelper.Value(Doc.DocumentElement, "ASC_Order"))
                    {
                        if (Params.ASCSubOrder == null || Params.ASCSubOrder == "" || Params.ASCSubOrder == XmlHelper.Value(Doc.DocumentElement, "ASC_Sub-order"))
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
                                        NewSoil.ASCOrder = XmlHelper.Value(Doc.DocumentElement, "ASC_Order");
                                        NewSoil.ASCSubOrder = XmlHelper.Value(Doc.DocumentElement, "ASC_Sub-order");
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
                throw err;
            }
            Reader.Close();
            Connection.Close();

            // Sort the soils and return them.
            Soils.Sort(CompareSoilLocations);

            //StreamWriter Out = new StreamWriter("D:\\Websites\\FILES\\Transfer\\ApsoilWeb.txt", true);
            //Out.WriteLine("Number of soils added:" + Soils.Count.ToString());
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
            List<SoilBasicInfo> Soils = new List<SoilBasicInfo>();
            SqlConnection Connection = Open();
            SqlDataReader Reader = null;
            try
            {
                XmlDocument Doc = new XmlDocument();

                SqlCommand Command = new SqlCommand("SELECT Name, XML FROM Soils WHERE IsApsoil=1", Connection);
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
                        Soils.Add(NewSoil);
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

            return Soils.ToArray();
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

                SqlCommand Command = new SqlCommand("SELECT Name, XML FROM Soils", Connection);
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




    }
}
