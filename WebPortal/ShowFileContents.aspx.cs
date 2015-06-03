using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;

namespace Apsoil
{
    public partial class ShowFileContents : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Clear();
            Response.ContentType = "text/plain";
            Response.ContentEncoding = System.Text.Encoding.UTF8;

            if (Request.QueryString["FileName"] != null)
            {
                string fileName = Request.QueryString["FileName"];

                if (File.Exists(fileName))
                {
                    StreamReader reader = new StreamReader(fileName);
                    string contents = reader.ReadToEnd();
                    Response.Write(contents);
                    reader.Close();
                    File.Delete(fileName);
                }
            }
            Response.End();
        }
    }
}