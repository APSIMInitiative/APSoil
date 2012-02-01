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
using KML_22_Beta1;
using KML_22_Beta1.KML_22_Beta1_Utils;
using System.Xml;
using CSGeneral;
using System.Collections.Generic;
using ApsimFile;

// To test the Google Earth KML extract put this into a browser:
// http://www.apsim.info/ApsoilWeb/ApsoilKML.aspx?Mode=1


namespace Apsoil
{


    /// <summary>
    /// The apsim.info web site calls this class with NO mode when the user has
    /// requested the KMZ file. The KMZ that gets returned then calls this
    /// with a Mode of 1. We then need to build the KML dynamically and return it.
    /// </summary>
    public partial class KML : System.Web.UI.Page
    {

        /// <summary>
        /// We're about to render the page - go work out what we need to do by looking
        /// at the Mode response variable.
        /// </summary>
        protected void Page_PreRender(object sender, EventArgs e)
        {
            Response.Clear();
            Response.AppendHeader("Content-Disposition", "inline;filename=APSRU_KML.kmz");
            Response.ContentType = "application/vnd.google-earth.kmz kmz";

            if (Request.QueryString["Mode"] == null)
                Response.BinaryWrite(MyDataKMZ());
            else
                Response.BinaryWrite(NetworkLinkData());
        }

        /// <summary>
        /// This property returns our URL path on disk.
        /// </summary>
        private string ourPath
        {
            get
            {
                int PosOurName = Request.Url.AbsoluteUri.IndexOf("ApsoilKML.aspx");
                if (PosOurName == -1)
                    throw new Exception("Invalid request URL: " + Request.Url.AbsoluteUri);
                string URL = Request.Url.AbsoluteUri.Remove(PosOurName);
                return URL;
            }
        }

        /// <summary>
        /// Return the inital KMZ as requested by the apsim.info web site.
        /// </summary>
        private byte[] MyDataKMZ()
        {
            kml KmlContainer = new kml();

            Document KmlDoc = new Document();
            KmlDoc.Name.Text = "APSoil characterised soils";
            KmlDoc.Snippet.Text = "Fully characterised soil sites for use with APSIM and Yield Prophet.";
            KmlDoc.Description.UseCDATA = true;
            KmlDoc.Description.Text = "<p>The soil sites displayed are from APSoil</p>";
            KmlContainer.Documents.Add(KmlDoc);

            NetworkLink KmlNetworkLink = new NetworkLink("APSoil soils", "All characterised soils");
            KmlNetworkLink.Link.Href.Text = ourPath + "/ApsoilKML.aspx?Mode=1";
            KmlNetworkLink.Link.RefreshMode.Value = refreshModeEnum.onExpire;
            KmlDoc.Features.NetworkLinks.Add(KmlNetworkLink);

            return KmlContainer.GetKMZ("APSRU_Sites.kml");
        }

        /// <summary>
        /// This is called directly by Google Earth when it wants KML data.
        /// </summary>
        private byte[] NetworkLinkData()
        {
            kml KmlContent = new kml();
            Document KmlDoc = new Document("Test", "");
            KmlContent.Documents.Add(KmlDoc);

            KML_22_Beta1.Style APSRUIcon = new KML_22_Beta1.Style();
            APSRUIcon.IconStyle.Icon.Href.Text = "http://www.apsim.info/ApsoilWeb/shovel.png";
            APSRUIcon.IconStyle.Scale.Value = 0.7;
            APSRUIcon.LabelStyle.Scale.Value = 0.7;
            APSRUIcon.id = "APSRUIconID";
            KmlDoc.StyleSelector.Style.Add(APSRUIcon);

            Dictionary<string, Folder> Folders = new Dictionary<string, Folder>();

            ApsoilWeb.Service SoilsDB = new Apsoil.ApsoilWeb.Service();
            foreach (string Name in SoilsDB.SoilNames())
            {
                string FolderName = Name;
                if (FolderName.Contains("/"))
                    FolderName = FolderName.Substring(0, FolderName.LastIndexOf('/'));

                XmlDocument Doc = new XmlDocument();
                Doc.LoadXml(SoilsDB.SoilXML(Name));
                XmlNode SoilNode = Doc.DocumentElement;
                if (SoilNode != null)
                {
                    double Latitude;
                    double Longitude;
                    if (double.TryParse(Soil.Get(SoilNode, "Latitude").Value, out Latitude) &&
                        double.TryParse(Soil.Get(SoilNode, "Longitude").Value, out Longitude))
                    {
                        string BalloonDescription = "<p><b>" + XmlHelper.Name(SoilNode) + "</b></p><p><i>" 
                                                  + XmlHelper.Value(SoilNode, "Comments") + "</i></p>";

                        Soil.Variable DataSourceComments = Soil.Get(SoilNode, "DataSource");
                        if (DataSourceComments.Value != null && DataSourceComments.Value != "")
                            BalloonDescription += "<p><i>Data source: " + DataSourceComments.Value + "</i></p>";

                        BalloonDescription += "<img src=\"" + ourPath + "SoilChart.aspx?Name=" + Name + "\"/>";

                        BalloonDescription += "<p><a href=\"" + ourPath + "GetSoil.aspx?Name=" + Name + "\">Download soil in APSIM format (copy and paste contents to your simulation).</a></p>";
                        BalloonDescription += "<p><a name=\"link_id\" id=\"link_id\"  href=\"Download.html\" onclick=\"window.open('" + ourPath + "Excel.aspx?Name=" + Name + "');\">Download soil as an EXCEL spreadsheet</a></p>";

                        string SoilName = XmlHelper.Name(SoilNode);
                        Soil.Variable SoilNumber = Soil.Get(SoilNode, "ApsoilNumber");
                        if (SoilNumber.Value != "")
                            SoilName = SoilNumber.Value;
                        Placemark plmMyPlaceMark = new Placemark(SoilName,
                                                                 BalloonDescription,
                                                                 Latitude,
                                                                 Longitude,
                                                                 0, altitudeModeEnum.clampToGround);
                        plmMyPlaceMark.Description.UseCDATA = true;
                        plmMyPlaceMark.Description.Text = BalloonDescription;
                        plmMyPlaceMark.StyleUrl.Text = "#APSRUIconID";

                        Folder F = GetFolder(FolderName, Folders, KmlDoc);
                        F.Features.Placemarks.Add(plmMyPlaceMark);
                    }
                }
            }

            KmlContent.NetworkLinkControl.Expires.Value = DateTime.Now.AddDays(7);
            KmlContent.NetworkLinkControl.LinkDescription.Text = "Characterised sites - " + DateTime.Now.ToString("dd MMM yyyy HH:mm:ss");
            return KmlContent.GetKMZ("NetworkData.kml");
        }


        private Folder GetFolder(string FolderName, Dictionary<string, Folder> Folders, Document KmlDoc)
        {
            if (Folders.ContainsKey(FolderName))
                return Folders[FolderName];

            int PosSlash = FolderName.LastIndexOf('/');
            if (FolderName == "Soils")
            {
                Folder F = new Folder(FolderName); // The root folder.
                KmlDoc.Features.Folders.Add(F);
                Folders.Add(FolderName, F);
                return F;
            }
            else if (PosSlash == 0)
            {
                Folder F = new Folder(FolderName.Substring(1)); // The root folder.
                KmlDoc.Features.Folders.Add(F);
                Folders.Add(FolderName, F);
                return F;
            }
            else if (PosSlash == -1)
            {
                throw new Exception("Invalid folder name: " + FolderName);
            }

            else
            {
                string ParentFolderName = FolderName.Substring(0, FolderName.LastIndexOf('/'));
                Folder Parent = GetFolder(ParentFolderName, Folders, KmlDoc);

                string ChildFolderName = FolderName.Substring(FolderName.LastIndexOf('/') + 1);
                Folder ChildFolder = new Folder(ChildFolderName); // The root folder.
                Parent.Features.Folders.Add(ChildFolder);
                Folders.Add(FolderName, ChildFolder);
                return ChildFolder;
            }
        }


    }
}
