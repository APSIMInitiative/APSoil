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
            DoSearch(withPAWC:true);
        }


        /// <summary>Handles the Click event of the Find button2 control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void OnFindClick2(object sender, EventArgs e)
        {
            DoSearch(withPAWC: false);
        }

        /// <summary>Do a search for soils and display them.</summary>
        private void DoSearch(bool withPAWC)
        {
            using (ApsoilWeb.Service SoilsDB = new Apsoil.ApsoilWeb.Service())
            {
                List<double> thickness = new List<double>();
                List<double> PAWC = new List<double>();
                List<double> grav = new List<double>();

                GetThicknessAndPAWC(Thickness1, PAWC1, Grav1, ref thickness, ref PAWC, ref grav);
                GetThicknessAndPAWC(Thickness2, PAWC2, Grav2, ref thickness, ref PAWC, ref grav);
                GetThicknessAndPAWC(Thickness3, PAWC3, Grav3, ref thickness, ref PAWC, ref grav);
                GetThicknessAndPAWC(Thickness4, PAWC4, Grav4, ref thickness, ref PAWC, ref grav);
                GetThicknessAndPAWC(Thickness5, PAWC5, Grav5, ref thickness, ref PAWC, ref grav);
                GetThicknessAndPAWC(Thickness6, PAWC6, Grav6, ref thickness, ref PAWC, ref grav);
                GetThicknessAndPAWC(Thickness7, PAWC7, Grav7, ref thickness, ref PAWC, ref grav);
                GetThicknessAndPAWC(Thickness8, PAWC8, Grav8, ref thickness, ref PAWC, ref grav);
                GetThicknessAndPAWC(Thickness9, PAWC9, Grav9, ref thickness, ref PAWC, ref grav);
                GetThicknessAndPAWC(Thickness10, PAWC10, Grav10, ref thickness, ref PAWC, ref grav);

                string[] closestSoils;
                if (withPAWC)
                    closestSoils = SoilsDB.ClosestMatchingSoils(thickness.ToArray(), PAWC.ToArray(), grav.ToArray(), "Wheat", 10, CheckBox1.Checked, true);
                else
                    closestSoils = SoilsDB.ClosestMatchingSoils(thickness.ToArray(), null, grav.ToArray(), "Wheat", 10, CheckBox1.Checked, true);

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
        private void GetThicknessAndPAWC(TextBox thicknessBox, TextBox pawcBox, TextBox gravBox, ref List<double> thickness, ref List<double> PAWC, ref List<double> grav)
        {
            if (thicknessBox.Text != string.Empty)
            {
                thickness.Add(Convert.ToDouble(thicknessBox.Text) * 10);
                if (pawcBox.Text != string.Empty && pawcBox.Text != null)
                    PAWC.Add(Convert.ToDouble(pawcBox.Text));
                if (gravBox.Text != string.Empty && gravBox.Text != null)
                    grav.Add(Convert.ToDouble(gravBox.Text));
            }
        }
    }
}