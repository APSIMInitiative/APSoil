using System.IO.Compression;
using System.Text;
using SharpKml.Base;
using SharpKml.Dom;

namespace API.Services;

public static class KML
{
    /// <summary>
    /// Converts the soils to a KML string.
    /// </summary>
    public static byte[] ToKMZ(this API.Models.Folder folder)
    {
        var style = new Style
        {
            Id = "shovel_icon",
            Icon = new IconStyle() { Icon = new IconStyle.IconLink(new Uri("shovel.png", UriKind.Relative)), }
        };

        var f = new SharpKml.Dom.Folder
        {
            Name = "Soils",
        };
        f.AddStyle(style);

        folder.ToKML(f);


        // This is the root element of the file
        var kml = new Kml
        {
            Feature = f,
        };

        var serializer = new Serializer();
        serializer.Serialize(kml);

        using (var s = new MemoryStream())
        {
            using (var archive = new ZipArchive(s, ZipArchiveMode.Create, true))
            {
                var zipArchiveEntry = archive.CreateEntry("soils.kml");
                using (var zipStream = zipArchiveEntry.Open())
                {
                    var bytes = Encoding.UTF8.GetBytes(serializer.Xml);
                    zipStream.Write(bytes, 0, bytes.Length);
                }
                zipArchiveEntry = archive.CreateEntry("shovel.png");
                using (var stream = typeof(Extensions).Assembly.GetManifestResourceStream("API.shovel.png"))
                using (var zipStream = zipArchiveEntry.Open())
                    stream.CopyTo(zipStream);
            }
            return s.ToArray();
        }
    }

    /// <summary>
    /// Convert a folder to KML.
    /// </summary>
    /// <param name="folder">The folder to convert.</param>
    /// <param name="kmlFolder">The KML folder to add features to.</param>
    private static void ToKML(this Models.Folder folder, SharpKml.Dom.Folder kmlFolder)
    {
        foreach (var soil in folder.Soils)
            kmlFolder.AddFeature(soil.ToPlacemark());
        if (folder.Folders != null)
            foreach (var subFolder in folder.Folders)
            {
                var sub = new SharpKml.Dom.Folder { Name = subFolder.Name };
                subFolder.ToKML(sub);
                kmlFolder.AddFeature(sub);
            }
    }

    /// <summary>
    /// Convert a soil to a KML placemark.
    /// </summary>
    /// <param name="soil">The soil to convert.</param>
    private static Placemark ToPlacemark(this Models.Soil soil)
    {
        return new Placemark
        {
            Name = soil.Name,
            StyleUrl = new Uri("#shovel_icon", UriKind.Relative),
            Geometry = new SharpKml.Dom.Point
            {
                Coordinate = new Vector(soil.Latitude, soil.Longitude)
            },
            Description = new Description()
            {
                Text = $"<p><b>{soil.Name}</b></p>" +
                       $"<p>{soil.DataSource}</p>" +
                       $"<a href=\"https://apsoil.apsim.info/search?FullName={soil.FullName}&output=CSV\">Download soil as .csv</a></p>" +
                       $"<a href=\"https://apsoil.apsim.info/search?FullName={soil.FullName}&output=FullSoilFile\">Download soil as XML</a></p>" +
                       $"<img src=\"https://apsoil.apsim.info/graph?FullName={soil.FullName}\" width=\"300\" height=\"400\"/><p>"
            }
        };
    }
}