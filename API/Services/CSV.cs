using System.Data;
using System.Globalization;

namespace API.Services;

public static class CSV
{
    /// <summary>
    /// Converts the soils to a CSV string.
    /// </summary>
    public static string ToCSV(this DataTable table)
    {
        StringWriter writer = new();
        DataTableToText(table, 0, ",", true, writer);
        return writer.ToString();
    }

    /// <summary>
    /// Write the specified DataTable to a CSV string, excluding the specified column names.
    /// </summary>
    static private void DataTableToText(DataTable data, int startColumnIndex, string delimiter, bool showHeadings, TextWriter writer, bool excelFriendly = false, string decimalFormatString = "F3")
    {
        // Convert the data table to a table of strings. This will make it easier for
        // calculating widths.
        DataTable stringTable = new DataTable();
        foreach (DataColumn col in data.Columns)
            stringTable.Columns.Add(col.ColumnName, typeof(string));
        foreach (DataRow row in data.Rows)
        {
            DataRow newRow = stringTable.NewRow();
            foreach (DataColumn column in data.Columns)
                newRow[column.Ordinal] = ConvertObjectToString(row[column], decimalFormatString);
            stringTable.Rows.Add(newRow);
        }

        //Sort Rows by SimulationName in alphabetical order
        if (stringTable.Columns.Contains("SimulationName"))
        {
            DataView dv = stringTable.DefaultView;
            dv.Sort = "SimulationName ASC";
            if (stringTable.Columns.Contains("Clock.Today"))
                dv.Sort += ", Clock.Today ASC";
            stringTable = dv.ToTable();
        }

        // Need to work out column widths
        List<int> columnWidths = new List<int>();
        foreach (DataColumn column in stringTable.Columns)
        {
            int width = column.ColumnName.Length;
            foreach (DataRow row in stringTable.Rows)
                width = System.Math.Max(width, row[column].ToString().Length);
            columnWidths.Add(width);
        }

        // Write out column headings.
        if (showHeadings)
        {
            for (int i = startColumnIndex; i < stringTable.Columns.Count; i++)
            {
                if (i > startColumnIndex)
                    writer.Write(delimiter);
                if (excelFriendly)
                    writer.Write(stringTable.Columns[i].ColumnName);
                else
                    writer.Write(stringTable.Columns[i].ColumnName);
            }
            writer.Write(Environment.NewLine);
        }

        // Write out each row.
        foreach (DataRow row in stringTable.Rows)
        {
            for (int i = startColumnIndex; i < stringTable.Columns.Count; i++)
            {
                if (i > startColumnIndex)
                    writer.Write(delimiter);
                if (excelFriendly)
                    writer.Write(row[i]);
                else
                    writer.Write(row[i]);
            }
            writer.Write(Environment.NewLine);
        }
    }

    /// <summary>
    /// Convert the specified object to a string.
    /// </summary>
    private static string ConvertObjectToString(object obj, string decimalFormatString)
    {
        if (obj is DateTime)
        {
            DateTime D = Convert.ToDateTime(obj, CultureInfo.InvariantCulture);
            return D.ToString("yyyy-MM-dd");
        }
        else if (obj is float || obj is double)
            return string.Format("{0:" + decimalFormatString + "}", obj);
        else
            return obj.ToString();
    }
}