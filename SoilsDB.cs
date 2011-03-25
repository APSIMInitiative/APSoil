using System.Collections.Generic;
using System.Data.SqlClient;
using CSGeneral;
using System.Xml;
namespace Apsoil
   {
   public class SoilsDB
      {
      private List<string> _SoilNames;
      SqlConnection Connection;
      XmlDocument Doc = new XmlDocument();
            

      /// <summary>
      /// Open the SoilsDB ready for use.
      /// </summary>
      public void Open()
         {
         // Create a connection to the database.

         // The first string is the debug version to run from Dean's computer.
         //string ConnectionString = "Server=www.apsim.info\\SQLEXPRESS;Database=APSoil;Trusted_Connection=True;";
         string ConnectionString = "Server=APSRUNET2\\SQLEXPRESS;Database=APSoil;Trusted_Connection=True;User ID=APSRUNET2\\apsrunet;password=CsiroDMZ!";
         
         Connection = new SqlConnection(ConnectionString);
         Connection.Open();
         }

      /// <summary>
      /// Close the SoilsDB connection
      /// </summary>
      public void Close()
         {
         Connection.Close();
         }

      /// <summary>
      /// Get a list of names from table.
      /// </summary>
      public List<string> SoilNames
         {
         get 
            {
            if (_SoilNames == null)
               {
               _SoilNames = new List<string>();

               SqlCommand Command = new SqlCommand("SELECT Name FROM Soils", Connection);
               SqlDataReader Reader = Command.ExecuteReader();
               while (Reader.Read())
                  _SoilNames.Add(Reader["Name"].ToString());
               Reader.Close();
               }
            return _SoilNames;
            }
         }

      /// <summary>
      /// Delete all soils.
      /// </summary>
      public void DeleteAllSoils()
         {
         SqlCommand Cmd = new SqlCommand("DELETE FROM Soils", Connection);
         Cmd.ExecuteNonQuery();
         }

      /// <summary>
      /// Add a soil to the DB, updating the existing one if necessary.
      /// </summary>
      public void AddSoil(string Name, string XML)
         {
         string SQL;
         if (SoilNames.Contains(Name))
            SQL = "UPDATE Soils SET XML = @XML WHERE Name = @Name";
         else
            SQL = "INSERT INTO Soils (Name, XML) VALUES (@Name, @XML)";

         SqlCommand Cmd = new SqlCommand(SQL, Connection);
         Cmd.Parameters.Add(new SqlParameter("@Name", Name));
         Cmd.Parameters.Add(new SqlParameter("@XML", XML));
         Cmd.ExecuteNonQuery();
         }

      /// <summary>
      /// Return the soil node for the specified soil. Will return
      /// null if soil was found.
      /// </summary>
      public XmlNode GetSoil(string Name)
         {
         SqlCommand Command = new SqlCommand("SELECT XML FROM Soils WHERE Name = @Name", Connection);
         Command.Parameters.Add(new SqlParameter("@Name", Name));
         SqlDataReader Reader = Command.ExecuteReader();
         if (Reader.Read())
            {
            Doc.LoadXml(Reader["XML"].ToString());
            Reader.Close();
            return Doc.DocumentElement;
            }

         Reader.Close();
         return null;
         }

      }
   }