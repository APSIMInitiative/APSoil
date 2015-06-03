using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Xml;
using ApsimFile;
using Newtonsoft.Json;
using CSGeneral;

namespace Apsoil
{
    /// <summary>
    /// The main APSOIL web form.
    /// </summary>
    public partial class WebForm1 : System.Web.UI.Page
    {
        /// <summary>Handles the Load event of the Page control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            using (ApsoilWeb.Service soilsDB = new Apsoil.ApsoilWeb.Service())
            {
                List<string> allSoils = new List<string>();
                allSoils.AddRange(soilsDB.SoilNames());
                allSoils.Sort();

                if (Request.QueryString["Paths"] != null)
                {
                    allSoils.Clear();
                    string[] paths = Request.QueryString["Paths"].Split(";".ToCharArray());
                    allSoils.AddRange(paths);
                }

                List<string> selectedItems = GetSelectedItems();

                Label.Text = "Number of soils: " + allSoils.Count.ToString();
                ListBox.Items.Clear();
                foreach (string soilName in allSoils)
                {
                    ListItem item = new ListItem(soilName);
                    item.Selected = selectedItems.Contains(soilName);
                    ListBox.Items.Add(item);
                }
            }
        }

        /// <summary>Handles the Click event of the show all button</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void OnShowAllClick(object sender, EventArgs e)
        {
            Response.Redirect("Apsoil.aspx");
        }

        /// <summary>Handles the "Closest PAWC button click event</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void OnFindClosestClick(object sender, EventArgs e)
        {
            Response.Redirect("FindClosest.aspx");
        }

        /// <summary>Uploads an APSoil soils file.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void UploadClick(object sender, EventArgs e)
        {
            Response.Redirect("UploadApsoilSoilsFile.aspx");
        }

        /// <summary>User has clicked the KML button.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void KMLClick(object sender, EventArgs e)
        {
            Response.Redirect("ApsoilKML.aspx");
        }

        /// <summary>User has clicked on show XML button.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ShowXMLClick(object sender, EventArgs e)
        {
            XmlDocument doc = new XmlDocument();
            doc.AppendChild(doc.CreateElement("folder"));
            XmlHelper.SetAttribute(doc.DocumentElement, "name", "Soils"); 
            using (ApsoilWeb.Service soilsDB = new Apsoil.ApsoilWeb.Service())
            {
                foreach (string path in GetSelectedItems())
                {
                    XmlDocument localXml = new XmlDocument();
                    localXml.LoadXml(soilsDB.SoilXML(path));
                    doc.DocumentElement.AppendChild(doc.ImportNode(localXml.DocumentElement, true));
                }
            }
            StringWriter writer = new StringWriter();
            writer.Write(XmlHelper.FormattedXML(doc.DocumentElement.OuterXml));
            ShowString(writer.ToString());
        }

        /// <summary>User has clicked show JSON button.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ShowJSONClick(object sender, EventArgs e)
        {
            StringWriter writer = new StringWriter();
            writer.WriteLine("[");
            
            using (ApsoilWeb.Service soilsDB = new Apsoil.ApsoilWeb.Service())
            {
                List<string> selectedPaths = GetSelectedItems();
                foreach (string path in selectedPaths)
                {
                    writer.Write(soilsDB.SoilAsJson(path));
                    if (selectedPaths.Last() != path)
                        writer.WriteLine(",");
                    else
                        writer.WriteLine("");
                }
            }
            writer.WriteLine("]");
            ShowString(writer.ToString());
        }

        /// <summary>Handles the Click event of the show paths button.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void OnShowPathsClick(object sender, EventArgs e)
        {
            StringWriter writer = new StringWriter();
            foreach (ListItem item in ListBox.Items)
            {
                if (item.Selected)
                    writer.WriteLine(item.Text);
            }
            ShowString(writer.ToString());
        }

        /// <summary>
        /// Called when user clicks on soil chart button.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void OnSoilChart(object sender, EventArgs e)
        {
            string SelectedName = ListBox.SelectedValue;
            Response.Redirect("http://www.apsim.info/ApsoilWeb/SoilChart.aspx?Name=" + SelectedName);
        }

        /// <summary>Handles the Click event of the 'upload a soil' button</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void OnUploadSoilClick(object sender, EventArgs e)
        {
            Response.Redirect("UploadApsoilSoilsFile.aspx?SoilPath=" + ListBox.SelectedValue);
        }

        /// <summary>Handles the TextChanged event of the FilterTextBox control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void OnFilterTextBoxChanged(object sender, EventArgs e)
        {
            if (FilterTextBox.Text != null)
            {
                string filterString = FilterTextBox.Text.ToLower();
                string soilNamesToShow = string.Empty;
                foreach (ListItem item in ListBox.Items)
                {
                    if (item.Text.ToLower().Contains(filterString))
                    {
                        string itemPath = item.Text;
                        if (soilNamesToShow != string.Empty)
                            soilNamesToShow += ";";
                        soilNamesToShow += itemPath;
                    }
                }
                Response.Redirect("Apsoil.aspx?Paths=" + soilNamesToShow);
            }
        }


        #region Privates
        /// <summary>Gets the selected items in the listbox.</summary>
        /// <returns></returns>
        private List<string> GetSelectedItems()
        {
            List<string> selectedItems = new List<string>();
            foreach (ListItem item in ListBox.Items)
            {
                if (item.Selected)
                    selectedItems.Add(item.Text);
            }
            return selectedItems;
        }


        /// <summary>Shows the specified string to the user</summary>
        /// <param name="st">The st.</param>
        private void ShowString(string st)
        {
            string fileName = Path.GetTempFileName();

            StreamWriter writer = new StreamWriter(fileName);
            writer.Write(st);
            writer.Close();

            string SelectedName = ListBox.SelectedValue;
            Response.Redirect("ShowFileContents.aspx?FileName=" + fileName);
        }
        #endregion


    }
}