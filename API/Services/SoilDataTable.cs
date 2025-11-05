using System.Data;
using APSIM.Numerics;

namespace API.Services;

public static class SoilDataTable
{
    private static string[] cropList = {
                                        "fieldpea", "mungbean", "sunflower", "fababean", "lucerne", "maize",
                                        "perennialgrass", "cowpea", "navybean", "peanut", "pigeonpea", "soybean",
                                        "stylo", "sugar", "lablab", "millet", "triticale", "weed", "medic",
                                        "Lupins", "lentils", "oatenhay", "broccoli", "peas", "Vetch", "potatoes",
                                        "poppy", "butterflypea", "burgundybean", "desmanthus_v", "centro", "caatingastylo",
                                        "brazilianstylo", "desmanthus_per", "rice", "mustard", "chickory"};

    /// <summary>
    /// Converts the soils to a DataTable
    /// </summary>
    public static DataTable ToDataTable(this Models.Soil soil)
    {
        DataTable table = new();

        int startRow = table.Rows.Count;
        int numValues = Math.Max(soil.Water.Thickness.Length, soil.Analysis.Thickness.Length);

        double[] layerNo = new double[soil.Water.Thickness.Length];
        for (int i = 1; i <= soil.Water.Thickness.Length; i++)
            layerNo[i - 1] = i;

        SetStringValue(table, "Name", soil.Name, startRow, numValues);
        SetDoubleValue(table, "RecordNo", soil.RecordNumber, startRow, numValues);
        SetStringValue(table, "Country", soil.Country, startRow, numValues);
        SetStringValue(table, "State", soil.State, startRow, numValues);
        SetStringValue(table, "Region", soil.Region, startRow, numValues);
        SetStringValue(table, "NearestTown", soil.NearestTown, startRow, numValues);
        SetStringValue(table, "Site", soil.Site, startRow, numValues);
        SetStringValue(table, "APSoilNumber", soil.ApsoilNumber, startRow, numValues);
        SetStringValue(table, "Soil type texture or other descriptor", soil.SoilType, startRow, numValues);
        SetStringValue(table, "Local name", soil.LocalName, startRow, numValues);
        SetStringValue(table, "ASC_Order", soil.ASCOrder, startRow, numValues);
        SetStringValue(table, "ASC_Sub-order", soil.ASCSubOrder, startRow, numValues);
        SetDoubleValue(table, "Latitude", soil.Latitude, startRow, numValues);
        SetDoubleValue(table, "Longitude", soil.Longitude, startRow, numValues);
        SetStringValue(table, "LocationAccuracy", soil.LocationAccuracy, startRow, numValues);
        SetIntegerValue(table, "YearOfSampling", soil.YearOfSampling, startRow, numValues);
        SetStringValue(table, "DataSource", soil.DataSource, startRow, numValues);
        SetStringValue(table, "Comments", soil.Comments, startRow, numValues);
        SetStringValue(table, "NaturalVegetation", soil.NaturalVegetation, startRow, numValues);
        SetDoubleValues(table, "LayerNo", layerNo, startRow);
        SetDoubleValues(table, "Thickness (mm)", soil.Water.Thickness, startRow);
        SetDoubleValues(table, "BD (g/cc)", soil.Water.BD, startRow);
        SetCodeValues(table, "BDCode", soil.Water.BDMetadata, startRow);
        SetDoubleValues(table, "Rocks (%)", soil.Analysis.Rocks, startRow);
        SetCodeValues(table, "RocksCode", soil.Analysis.RocksMetadata, startRow);
        SetStringValues(table, "Texture", soil.Analysis.Texture, startRow);
        SetCodeValues(table, "TextureCode", soil.Analysis.TextureMetadata, startRow);
        SetDoubleValues(table, "SAT (mm/mm)", soil.Water.SAT, startRow);
        SetCodeValues(table, "SATCode", soil.Water.SATMetadata, startRow);
        SetDoubleValues(table, "DUL (mm/mm)", soil.Water.DUL, startRow);
        SetCodeValues(table, "DULCode", soil.Water.DULMetadata, startRow);
        SetDoubleValues(table, "LL15 (mm/mm)", soil.Water.LL15, startRow);
        SetCodeValues(table, "LL15Code", soil.Water.LL15Metadata, startRow);
        SetDoubleValues(table, "Airdry (mm/mm)", soil.Water.AirDry, startRow);
        SetCodeValues(table, "AirdryCode", soil.Water.AirDryMetadata, startRow);
        SetCropValues(table, "wheat", soil, startRow);
        SetCropValues(table, "barley", soil, startRow);
        SetCropValues(table, "oats", soil, startRow);
        SetCropValues(table, "canola", soil, startRow);
        SetCropValues(table, "chickpea", soil, startRow);
        SetCropValues(table, "cotton", soil, startRow);
        SetCropValues(table, "sorghum", soil, startRow);
        SetDoubleValue(table, "SummerU", soil.SoilWater.SummerU, startRow, numValues);
        SetDoubleValue(table, "SummerCona", soil.SoilWater.SummerCona, startRow, numValues);
        SetDoubleValue(table, "WinterU", soil.SoilWater.WinterU, startRow, numValues);
        SetDoubleValue(table, "WinterCona", soil.SoilWater.WinterCona, startRow, numValues);
        SetStringValue(table, "SummerDate", "=\"" + soil.SoilWater.SummerDate + "\"", startRow, numValues);
        SetStringValue(table, "WinterDate", "=\"" + soil.SoilWater.WinterDate + "\"", startRow, numValues);
        SetDoubleValue(table, "Salb", soil.SoilWater.Salb, startRow, numValues);
        SetDoubleValue(table, "DiffusConst", soil.SoilWater.DiffusConst, startRow, numValues);
        SetDoubleValue(table, "DiffusSlope", soil.SoilWater.DiffusSlope, startRow, numValues);
        SetDoubleValue(table, "CN2Bare", soil.SoilWater.CN2Bare, startRow, numValues);
        SetDoubleValue(table, "CNRed", soil.SoilWater.CNRed, startRow, numValues);
        SetDoubleValue(table, "CNCov", soil.SoilWater.CNCov, startRow, numValues);
        SetDoubleValue(table, "RootCN", soil.SoilOrganicMatter.RootCN, startRow, numValues);
        SetDoubleValue(table, "RootWT", soil.SoilOrganicMatter.RootWt, startRow, numValues);
        SetDoubleValue(table, "SoilCN", soil.SoilOrganicMatter.SoilCN, startRow, numValues);
        SetDoubleValue(table, "EnrACoeff", soil.SoilOrganicMatter.EnrACoeff, startRow, numValues);
        SetDoubleValue(table, "EnrBCoeff", soil.SoilOrganicMatter.EnrBCoeff, startRow, numValues);
        SetDoubleValues(table, "SWCON (0-1)", soil.SoilWater.SWCON, startRow);
        SetDoubleValues(table, "FBIOM (0-1)", soil.SoilOrganicMatter.FBiom, startRow);
        SetDoubleValues(table, "FINERT (0-1)", soil.SoilOrganicMatter.FInert, startRow);
        SetDoubleValues(table, "KS (mm/day)", soil.Water.KS, startRow);
        SetDoubleValues(table, "ThicknessChem (mm)", soil.SoilOrganicMatter.Thickness, startRow);
        SetDoubleValues(table, "OC", soil.SoilOrganicMatter.OC, startRow);
        SetOCCodeValues(table, soil, startRow);
        SetDoubleValues(table, "EC (1:5 dS/m)", soil.Analysis.EC, startRow);
        SetCodeValues(table, "ECCode", soil.Analysis.ECMetadata, startRow);
        SetDoubleValues(table, "PH", soil.Analysis.PH, startRow);
        SetPHCodeValues(table, soil, startRow);
        SetDoubleValues(table, "CL (mg/kg)", soil.Analysis.CL, startRow);
        SetCodeValues(table, "CLCode", soil.Analysis.CLMetadata, startRow);
        SetDoubleValues(table, "CEC (cmol+/kg)", soil.Analysis.CEC, startRow);
        SetCodeValues(table, "CECCode", soil.Analysis.CECMetadata, startRow);
        SetDoubleValues(table, "ESP (%)", soil.Analysis.ESP, startRow);
        SetCodeValues(table, "ESPCode", soil.Analysis.ESPMetadata, startRow);
        SetDoubleValues(table, "ParticleSizeSand (%)", soil.Analysis.ParticleSizeSand, startRow);
        SetCodeValues(table, "ParticleSizeSandCode", soil.Analysis.ParticleSizeSandMetadata, startRow);
        SetDoubleValues(table, "ParticleSizeSilt (%)", soil.Analysis.ParticleSizeSilt, startRow);
        SetCodeValues(table, "ParticleSizeSiltCode", soil.Analysis.ParticleSizeSiltMetadata, startRow);
        SetDoubleValues(table, "ParticleSizeClay (%)", soil.Analysis.ParticleSizeClay, startRow);
        SetCodeValues(table, "ParticleSizeClayCode", soil.Analysis.ParticleSizeClayMetadata, startRow);

        foreach (string cropName in cropList)
            SetCropValues(table, cropName, soil, startRow);
        return table;
    }

    /// <summary>
    /// Set the PHCode column of the specified table.
    /// </summary>
    private static void SetPHCodeValues(DataTable table, Models.Soil soil, int startRow)
    {
        string[] codes = MetaDataToCode(soil.Analysis.PHMetadata);
        if (codes != null)
            for (int i = 0; i < codes.Length; i++)
                codes[i] += " (" + soil.Analysis.PHUnits + ")";
        SetStringValues(table, "PHCode", codes, startRow);
    }

    /// <summary>
    /// Set the OCCode column of the specified table.
    /// </summary>
    private static void SetOCCodeValues(DataTable table, Models.Soil soil, int startRow)
    {
        string[] codes = MetaDataToCode(soil.SoilOrganicMatter.OCMetadata);
        if (codes != null)
            for (int i = 0; i < codes.Length; i++)
                codes[i] += " (" + soil.SoilOrganicMatter.OCUnits + ")";
        SetStringValues(table, "OCCode", codes, startRow);
    }

    /// <summary>
    /// Set a column of metadata values for the specified column.
    /// </summary>
    private static void SetCodeValues(DataTable table, string columnName, string[] metadata, int startRow)
    {
        string[] codes = MetaDataToCode(metadata);
        SetStringValues(table, columnName, codes, startRow);
    }

    /// <summary>
    /// Set the crop values in the table for the specified crop name.
    /// </summary>
    private static void SetCropValues(DataTable table, string cropName, Models.Soil soil, int startRow)
    {
        var cropNames = soil?.Water?.SoilCrops?.Select(sc => sc.Name);
        if (cropNames.Contains(cropName, StringComparer.CurrentCultureIgnoreCase))
        {
            SetDoubleValues(table, cropName + " ll (mm/mm)", soil.Crop(cropName).LL, startRow);
            SetCodeValues(table, cropName + " llCode", soil.Crop(cropName).LLMetadata, startRow);
            SetDoubleValues(table, cropName + " kl (/day)", soil.Crop(cropName).KL, startRow);
            SetDoubleValues(table, cropName + " xf (0-1)", soil.Crop(cropName).XF, startRow);
        }
        else if (!table.Columns.Contains(cropName + " ll (mm/mm)"))
        {
            table.Columns.Add(cropName + " ll (mm/mm)", typeof(double));
            table.Columns.Add(cropName + " llCode", typeof(string));
            table.Columns.Add(cropName + " kl (/day)", typeof(double));
            table.Columns.Add(cropName + " xf (0-1)", typeof(double));
        }
    }

    /// <summary>
    /// Set a column of double values in the specified table.
    /// </summary>
    private static void SetDoubleValues(DataTable table, string columnName, double[] values, int startRow)
    {
        if (MathUtilities.ValuesInArray(values))
            AddColumn(table, columnName, values, startRow, values.Length);
        else if (!table.Columns.Contains(columnName))
            table.Columns.Add(columnName, typeof(double));
    }

    /// <summary>
    /// Set a column of string values in the specified table.
    /// </summary>
    private static void SetStringValues(DataTable table, string columnName, string[] values, int startRow)
    {
        if (MathUtilities.ValuesInArray(values))
            AddColumn(table, columnName, values, startRow, values.Length);
        else if (!table.Columns.Contains(columnName))
            table.Columns.Add(columnName, typeof(string));
    }

    /// <summary>
    /// Set a column to the specified Value a specificed numebr of times.
    /// </summary>
    private static void SetDoubleValue(DataTable table, string columnName, double value, int startRow, int numValues)
    {
        double[] values = new double[numValues];
        for (int i = 0; i < numValues; i++)
            values[i] = value;
        SetDoubleValues(table, columnName, values, startRow);
    }

    /// <summary>
    /// Set a column to the specified Value a specificed numebr of times.
    /// </summary>
    private static void SetIntegerValue(DataTable table, string columnName, int value, int startRow, int numValues)
    {
        double[] values = new double[numValues];
        for (int i = 0; i < numValues; i++)
            values[i] = value;
        SetDoubleValues(table, columnName, values, startRow);
    }

    /// <summary>
    /// Set a column to the specified Value a specificed numebr of times.
    /// </summary>
    private static void SetStringValue(DataTable table, string volumnName, string value, int startRow, int numValues)
    {
        string[] values = CreateStringArray(value, numValues);
        SetStringValues(table, volumnName, values, startRow);
    }

    /// <summary>
    /// Add a column of values to the specified data table
    /// </summary>
    static private void AddColumn<T>(DataTable table, string columnName, IEnumerable<T> values, int startRow, int count)
    {
        if (table.Columns.IndexOf(columnName) == -1)
            table.Columns.Add(columnName, typeof(T));

        if (values == null)
            return;

        int row = startRow;
        foreach (var value in values)
        {
            while (row >= table.Rows.Count)
                table.Rows.Add(table.NewRow());

            if (IsValid(value))
                table.Rows[row][columnName] = value;
            row++;
        }
    }

    /// <summary>
    /// Is a value a valid string or double?
    /// </summary>
    /// <param name="value">The value to test.</param>
    static private bool IsValid(object value)
    {
        return value != null &&
                (value.GetType() == typeof(string) && !string.IsNullOrEmpty((string)value) ||
                value.GetType() == typeof(double) && !double.IsNaN((double)value));
    }

    /// <summary>
    /// Create a string array containing the specified number of values.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="numValues"></param>
    /// <returns></returns>
    private static string[] CreateStringArray(string value, int numValues)
    {
        string[] Arr = new string[numValues];
        for (int i = 0; i < numValues; i++)
            Arr[i] = value;
        return Arr;
    }

    /// <summary>
    /// Convert a metadata into an abreviated code.
    /// </summary>
    static public string[] MetaDataToCode(string[] metadata)
    {
        if (metadata == null)
            return null;

        string[] codes = new string[metadata.Length];
        for (int i = 0; i < metadata.Length; i++)
            if (metadata[i] == "Field measured and checked for sensibility")
                codes[i] = "FM";
            else if (metadata[i] == "Calculated from gravimetric moisture when profile wet but drained")
                codes[i] = "C_grav";
            else if (metadata[i] == "Estimated based on local knowledge")
                codes[i] = "E";
            else if (metadata[i] == "Unknown source or quality of data")
                codes[i] = "U";
            else if (metadata[i] == "Laboratory measured")
                codes[i] = "LM";
            else if (metadata[i] == "Volumetric measurement")
                codes[i] = "V";
            else if (metadata[i] == "Measured")
                codes[i] = "M";
            else if (metadata[i] == "Calculated from measured, estimated or calculated BD")
                codes[i] = "C_bd";
            else if (metadata[i] == "Developed using a pedo-transfer function")
                codes[i] = "C_pt";
            else
                codes[i] = metadata[i];
        return codes;
    }

}