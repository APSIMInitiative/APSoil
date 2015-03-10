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
using System.Xml;
using System.IO;

namespace Apsoil
{
    public partial class SearchSoil : System.Web.UI.Page
    {

        /// <summary>
        /// We're about to render the page - get the soil node and write it to the
        /// response.
        /// </summary>
        protected void Page_PreRender(object sender, EventArgs e)
        {
            Response.Clear();
            //Response.AppendHeader("Content-Disposition", "attachment; filename=Registrations.csv");
            Response.Buffer = false;
            Response.ContentType = "text/plain";

            if (Request.QueryString["Latitude"] != null &&
                Request.QueryString["Longitude"] != null &&
                Request.QueryString["Radius"] != null)
            {
                double Latitude = Convert.ToDouble(Request.QueryString["Latitude"]);
                double Longitude = Convert.ToDouble(Request.QueryString["Longitude"]);
                double Radius = Convert.ToDouble(Request.QueryString["Radius"]);
                string asc = Request.QueryString["asc"];

                ApsoilWeb.Service SoilsDB = new Apsoil.ApsoilWeb.Service();

                // Make sure the soil search method works. The lat/long below is for soil:Black Vertosol-Anchorfield (Brookstead No006)
                string SoilNamesXML = SoilsDB.SearchSoils(Latitude, Longitude, Radius, asc);

                Response.Write(SoilNamesXML);
                Response.SuppressContent = true;  // stops .net from writing it's stuff.
            }
        }

    }
}
