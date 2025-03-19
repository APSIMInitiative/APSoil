using System.Xml.Serialization;

public static class ResourceFile
{
    /// <summary>Convert an resource file to an object.</summary>
    /// <param name="xml">The resource name.</param>
    /// <returns>The object instance on success, otherwise throws.</returns>
    public static T FromResourceXML<T>(string resourceName)
    {
        string xml = Get(resourceName);
        if (typeof(T) == typeof(API.Models.Folder))
            xml = xml.Replace("folder", "Folder");
        return FromXML<T>(xml);
    }

    /// <summary>
    /// Convert an resource file to an string.
    /// </summary>
    /// <param name="resourceName">The name of the resources</param>
    /// <returns>The string.</returns>
    public static string Get(string resourceName)
    {
        var assembly = typeof(ResourceFile).Assembly;
        using Stream stream = assembly.GetManifestResourceStream(resourceName);
        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }

    /// <summary>Convert an xml string to an object.</summary>
    /// <param name="xml">The resource name.</param>
    /// <returns>The object instance on success, otherwise throws.</returns>
    private static T FromXML<T>(string xml)
    {
        if (typeof(T) == typeof(API.Models.Folder))
            xml = xml.Replace("folder", "Folder");
        xml = xml.Replace("NaN", "0");
        var serializer = new XmlSerializer(typeof(T));
        return (T)serializer.Deserialize(new StringReader(xml));
    }
}