using APSIM.Shared.Utilities;

namespace Tests;

public class OldCodeTests
{
    public void Old()
    {
        var soil = ResourceFile.FromResourceXML<API.Models.Soil>("Tests.old.xml");

        /*RemoveMissingValues(ref Thickness, ref SW);
        Soil.Samples.Add(new Sample() { Thickness = Thickness, SW = SW });
        if (IsGravimetric)
            Soil.Samples[0].SWUnits = Sample.SWUnitsEnum.Gravimetric;
        */
        //double[] PAWCmm = APSIM.Shared.Utilities.MathUtilities.Multiply(Soil.PAWCrop(CropName), soil.Water.Thickness);

        double[] SW = [ 0.4, 0.3 ];
        double[] Thickness = [ 100, 500 ];
        var wheat = soil.Water.SoilCrops.FirstOrDefault(c => c.Name == "wheat");
        var SWAtWaterThickness = SWMapped(soil, SWVolumetric(soil, SW), Thickness, soil.Water.Thickness.ToArray());
        double[] PAWCmm = SoilUtilities.CalcPAWC(soil.Water.Thickness,
                                                 LLMapped(soil, "wheat", soil.Water.Thickness.ToArray()),
                                                 SWMapped(soil, SWAtWaterThickness, soil.Water.Thickness.ToArray(), soil.Water.Thickness.ToArray()),
                                                 XFMapped(soil, "wheat", soil.Water.Thickness.ToArray()));
        PAWCmm = MathUtilities.Multiply(PAWCmm, soil.Water.Thickness.ToArray());
        double PAWmm = APSIM.Shared.Utilities.MathUtilities.Sum(PAWCmm);
    }
    public enum SWUnitsEnum { Volumetric, Gravimetric, mm }

    /// <summary>
    /// Return SW. Units: vol mm/mm.
    /// </summary>
    public double[] SWVolumetric(API.Models.Soil Soil, double[] SW)
    {
        if (SW == null) return null;
        double[] OriginalValues = (double[]) SW.Clone();
        //SWUnitsEnum originalUnits = SWUnits;
        SWUnitsSet(SWUnitsEnum.Volumetric, Soil);
        double[] Values = (double[]) SW.Clone();
        SW = OriginalValues;
        //SWUnits = originalUnits;
        return Values;
    }

    public void SWUnitsSet(SWUnitsEnum ToUnits, API.Models.Soil Soil)
    {
        /*if (ToUnits != SWUnits && SW != null)
        {
            // convert the numbers
            if (SWUnits == SWUnitsEnum.Volumetric)
            {
                if (ToUnits == SWUnitsEnum.Gravimetric)
                    SW = MathUtility.Divide(SW, Soil.BDMapped(Thickness));
                else if (ToUnits == SWUnitsEnum.mm)
                    SW = MathUtility.Multiply(SW, Thickness);
            }
            else if (SWUnits == SWUnitsEnum.Gravimetric)
            {
                if (ToUnits == SWUnitsEnum.Volumetric)
                    SW = MathUtility.Multiply(SW, Soil.BDMapped(Thickness));
                else if (ToUnits == SWUnitsEnum.mm)
                    SW = MathUtility.Multiply(MathUtility.Multiply(SW, Soil.BDMapped(Thickness)), Thickness);
            }
            else
            {
                if (ToUnits == SWUnitsEnum.Volumetric)
                    SW = MathUtility.Divide(SW, Thickness);
                else if (ToUnits == SWUnitsEnum.Gravimetric)
                    SW = MathUtility.Divide(MathUtility.Divide(SW, Thickness), Soil.BDMapped(Thickness));
            }
        }
        SWUnits = ToUnits;*/
    }

    /// <summary>
    /// SW - mapped to the specified layer structure. Units: mm/mm
    /// </summary>
    public double[] SWMapped(API.Models.Soil soil, double[] Values, double[] Thicknesses, double[] ToThickness)
    {
        if (Values == null)
            return new double[0];
        if (Thicknesses == ToThickness)
            return Values;

        // Get the last item in the sw array and its indx.
        double LastSW = LastValue(Values);
        double LastThickness = LastValue(Thicknesses);
        int LastIndex = Values.Length - 1;

        Array.Resize(ref Thicknesses, Thicknesses.Length + 3); // Increase array size by 3.
        Array.Resize(ref Values, Values.Length + 3);

        Thicknesses[LastIndex + 1] = LastThickness;
        Thicknesses[LastIndex + 2] = LastThickness;
        Thicknesses[LastIndex + 3] = 3000;

        Values[LastIndex + 1] = LastSW * 0.8;
        Values[LastIndex + 2] = LastSW * 0.4;
        Values[LastIndex + 3] = 0.0; // This will be constrained below to crop LL below.

        // Get the first crop ll or ll15.
        double[] LowerBound;
        if (soil.Water.SoilCrops.Count > 0)
            LowerBound = LLMapped(soil, soil.Water.SoilCrops[0].Name, Thicknesses).ToArray();
        else
            LowerBound = LL15Mapped(soil, Thicknesses);
        if (LowerBound == null)
            throw new Exception("Cannot find crop lower limit or LL15 in soil");

        // Make sure all SW values below LastIndex don't go below CLL.
        for (int i = LastIndex + 1; i < Thicknesses.Length; i++)
            if (i < Values.Length && i < LowerBound.Length)
                Values[i] = Math.Max(Values[i], LowerBound[i]);

        return Map(Values, Thicknesses, ToThickness, MapType.Concentration);
    }

    /// <summary>
    /// Crop lower limit mapped. Units: mm/mm
    /// </summary>
    public IReadOnlyList<double> LLMapped(API.Models.Soil soil, string CropName, double[] ToThickness)
    {
        var SoilCrop = soil.Water.SoilCrops.FirstOrDefault(c => c.Name == CropName);
        if (MathUtilities.AreEqual(SoilCrop.Thickness, ToThickness))
            return SoilCrop.LL;
        if (MathUtilities.AreEqual(SoilCrop.Thickness, ToThickness))
            return SoilCrop.LL;
        double[] Values = Map(SoilCrop.LL, SoilCrop.Thickness, ToThickness, MapType.Concentration, LastValue(SoilCrop.LL));
        if (Values == null) return null;
        double[] AirDry = AirDryMapped(soil, ToThickness);
        double[] DUL = DULMapped(soil, ToThickness);
        if (AirDry == null || DUL == null)
            return null;
        for (int i = 0; i < Values.Length; i++)
        {
            if (i < AirDry.Length)
                Values[i] = Math.Max(Values[i], AirDry[i]);
            if (i < DUL.Length)
                Values[i] = Math.Min(Values[i], DUL[i]);
        }
        return Values;
    }

    /// <summary>
    /// Crop XF mapped. Units: 0-1
    /// </summary>
    internal IReadOnlyList<double> XFMapped(API.Models.Soil soil, string CropName, double[] ToThickness)
    {
        var SoilCrop = soil.Water.SoilCrops.FirstOrDefault(c => c.Name == CropName);
        if (MathUtilities.AreEqual(SoilCrop.Thickness, ToThickness))
            return SoilCrop.XF;
        return Map(SoilCrop.XF, SoilCrop.Thickness, ToThickness, MapType.Concentration, LastValue(SoilCrop.XF));
    }

    /// <summary>
    /// AirDry - mapped to the specified layer structure. Units: mm/mm
    /// </summary>
    public double[] AirDryMapped(API.Models.Soil soil, double[] ToThickness)
    {
        return Map(soil.Water.AirDry, soil.Water.Thickness, ToThickness, MapType.Concentration, soil.Water.AirDry.Last());
    }

    /// <summary>
    /// Lower limit 15 bar - mapped to the specified layer structure. Units: mm/mm
    /// </summary>
    public double[] LL15Mapped(API.Models.Soil soil, double[] ToThickness)
    {
        return Map(soil.Water.LL15, soil.Water.Thickness, ToThickness, MapType.Concentration, soil.Water.LL15.Last());
    }

    /// <summary>
    /// Drained upper limit - mapped to the specified layer structure. Units: mm/mm
    /// </summary>
    public double[] DULMapped(API.Models.Soil soil, double[] ToThickness)
    {
        return Map(soil.Water.DUL, soil.Water.Thickness, ToThickness, MapType.Concentration, soil.Water.DUL.Last());
    }

    /// <summary>
    /// Bulk density - mapped to the specified layer structure. Units: mm/mm
    /// </summary>
    internal double[] BDMapped(API.Models.Soil soil, double[] ToThickness)
    {
        if (soil.Water.BD == null || soil.Water.BD.Count == 0)
            return null;
        else
            return Map(soil.Water.BD, soil.Water.Thickness, ToThickness, MapType.Concentration, soil.Water.BD.Last());
    }

    /// <summary>
    /// Return the last value that isn't a missing value.
    /// </summary>
    private double LastValue(IReadOnlyList<double> Values)
    {
        if (Values == null) return double.NaN;
        for (int i = Values.Count - 1; i >= 0; i--)
            if (!double.IsNaN(Values[i]))
                return Values[i];
        return double.NaN;
    }

    private enum MapType { Mass, Concentration, UseBD }

    /// <summary>
    /// Map soil variables from one layer structure to another.
    /// </summary>
    private double[] Map(IReadOnlyList<double> FValues, IReadOnlyList<double> FThickness,
                        IReadOnlyList<double> ToThickness, MapType MapType,
                        double DefaultValueForBelowProfile = double.NaN)
    {
        if (FValues == null || FThickness == null)
            return null;

        double[] FromThickness = MathUtilities.RemoveMissingValuesFromBottom((double[]) FThickness.ToArray().Clone());
        double[] FromValues = (double[])FValues.ToArray().Clone();

        if (FromValues == null)
            return null;

        // remove missing layers.
        for (int i = 0; i < FromValues.Length; i++)
        {
            if (double.IsNaN(FromValues[i]) || i >= FromThickness.Length || double.IsNaN(FromThickness[i]))
            {
                FromValues[i] = double.NaN;
                if (i == FromThickness.Length)
                    Array.Resize(ref FromThickness, i + 1);
                FromThickness[i] = double.NaN;
            }
        }
        FromValues = MathUtilities.RemoveMissingValuesFromBottom(FromValues);
        FromThickness = MathUtilities.RemoveMissingValuesFromBottom(FromThickness);

        if (MathUtilities.AreEqual(FromThickness, ToThickness.ToArray()))
            return FromValues;

        if (FromValues.Length != FromThickness.Length)
            return null;

        // Add the default value if it was specified.
        if (!double.IsNaN(DefaultValueForBelowProfile))
        {
            Array.Resize(ref FromThickness, FromThickness.Length + 1);
            Array.Resize(ref FromValues, FromValues.Length + 1);
            FromThickness[FromThickness.Length - 1] = 3000;  // to push to profile deep.
            FromValues[FromValues.Length - 1] = DefaultValueForBelowProfile;
        }

        // If necessary convert FromValues to a mass.
        if (MapType == MapType.Concentration)
            FromValues = MathUtilities.Multiply(FromValues, FromThickness);
        else if (MapType == MapType.UseBD)
        {
            throw new NotImplementedException("UseBD mapping not implemented yet.");
        /*    double[] BD = Water.BD;
            for (int Layer = 0; Layer < FromValues.Length; Layer++)
                FromValues[Layer] = FromValues[Layer] * BD[Layer] * FromThickness[Layer] / 100;
        */}

        // Remapping is achieved by first constructing a map of
        // cumulative mass vs depth
        // The new values of mass per layer can be linearly
        // interpolated back from this shape taking into account
        // the rescaling of the profile.

        double[] CumDepth = new double[FromValues.Length + 1];
        double[] CumMass = new double[FromValues.Length + 1];
        CumDepth[0] = 0.0;
        CumMass[0] = 0.0;
        for (int Layer = 0; Layer < FromThickness.Length; Layer++)
        {
            CumDepth[Layer + 1] = CumDepth[Layer] + FromThickness[Layer];
            CumMass[Layer + 1] = CumMass[Layer] + FromValues[Layer];
        }

        //look up new mass from interpolation pairs
        double[] ToMass = new double[ToThickness.Count];
        for (int Layer = 1; Layer <= ToThickness.Count; Layer++)
        {
            double LayerBottom = MathUtilities.Sum(ToThickness, 0, Layer, 0.0);
            double LayerTop = LayerBottom - ToThickness[Layer - 1];
            bool DidInterpolate;
            double CumMassTop = MathUtilities.LinearInterpReal(LayerTop, CumDepth,
                CumMass, out DidInterpolate);
            double CumMassBottom = MathUtilities.LinearInterpReal(LayerBottom, CumDepth,
                CumMass, out DidInterpolate);
            ToMass[Layer - 1] = CumMassBottom - CumMassTop;
        }

        // If necessary convert FromValues back into their former units.
        if (MapType == MapType.Concentration)
            ToMass = MathUtilities.Divide(ToMass, ToThickness.ToArray());
        else if (MapType == MapType.UseBD)
        {
            throw new NotImplementedException("UseBD mapping not implemented yet.");
            /*
            double[] BD = BDMapped(soil, ToThickness);
            for (int Layer = 0; Layer < FromValues.Length; Layer++)
                ToMass[Layer] = ToMass[Layer] * 100.0 / BD[Layer] / ToThickness[Layer];
            */
        }

        for (int i = 0; i < ToMass.Length; i++)
            if (double.IsNaN(ToMass[i]))
                ToMass[i] = 0.0;
        return ToMass;
    }
}