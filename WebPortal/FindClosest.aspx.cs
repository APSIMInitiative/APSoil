using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Apsoil
{
    public partial class FindClosest : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        /// <summary>Handles the Click event of the Find button control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void OnFindClick(object sender, EventArgs e)
        {
            using (ApsoilWeb.Service SoilsDB = new Apsoil.ApsoilWeb.Service())
            {
                List<double> thickness = new List<double>();
                List<double> PAWC = new List<double>();

                GetThicknessAndPAWC(Thickness1, PAWC1, ref thickness, ref PAWC);
                GetThicknessAndPAWC(Thickness2, PAWC2, ref thickness, ref PAWC);
                GetThicknessAndPAWC(Thickness3, PAWC3, ref thickness, ref PAWC);
                GetThicknessAndPAWC(Thickness4, PAWC4, ref thickness, ref PAWC);
                GetThicknessAndPAWC(Thickness5, PAWC5, ref thickness, ref PAWC);
                GetThicknessAndPAWC(Thickness6, PAWC6, ref thickness, ref PAWC);
                GetThicknessAndPAWC(Thickness7, PAWC7, ref thickness, ref PAWC);
                GetThicknessAndPAWC(Thickness8, PAWC8, ref thickness, ref PAWC);
                GetThicknessAndPAWC(Thickness9, PAWC9, ref thickness, ref PAWC);
                GetThicknessAndPAWC(Thickness10, PAWC10, ref thickness, ref PAWC);

                string[] closestSoils = SoilsDB.ClosestMatchingSoils(thickness.ToArray(), PAWC.ToArray(), "Wheat", 10);

                string soilNames = string.Empty;
                foreach (string path in closestSoils)
                {
                    if (soilNames != string.Empty)
                        soilNames += ";";

                    soilNames += path;
                }

                Response.Redirect("Apsoil.aspx?Paths=" + soilNames);
            }
        }

        /// <summary>Gets the thickness and pawc from the 2 text boxes.</summary>
        /// <param name="thicknessBox">The thickness box.</param>
        /// <param name="pawcBox">The pawc box.</param>
        /// <param name="thickness">The thickness list.</param>
        /// <param name="PAWC">The pawc list.</param>
        private void GetThicknessAndPAWC(TextBox thicknessBox, TextBox pawcBox, ref List<double> thickness, ref List<double> PAWC)
        {
            if (thicknessBox.Text != string.Empty && pawcBox.Text != string.Empty)
            {
                thickness.Add(Convert.ToDouble(thicknessBox.Text) * 10);
                PAWC.Add(Convert.ToDouble(pawcBox.Text));
            }
        }
    }
}