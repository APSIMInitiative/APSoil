using System.Xml.Serialization;

public static class ResourceFile
{
    /// <summary>Convert an resource file to an object.</summary>
    /// <param name="xml">The resource name.</param>
    /// <returns>The object instance on success, otherwise throws.</returns>
    public static T FromResourceXML<T>(string resourceName)
    {
        var assembly = typeof(ResourceFile).Assembly;
        using Stream stream = assembly.GetManifestResourceStream(resourceName);
        using StreamReader reader = new(stream);
        string xml = reader.ReadToEnd();

        if (typeof(T) == typeof(API.Models.Folder))
            xml = xml.Replace("folder", "Folder");
        return FromXML<T>(xml);
    }

    /// <summary>Convert an xml string to an object.</summary>
    /// <param name="xml">The resource name.</param>
    /// <returns>The object instance on success, otherwise throws.</returns>
    private static T FromXML<T>(string xml)
    {
        if (typeof(T) == typeof(API.Models.Folder))
            xml = xml.Replace("folder", "Folder");
        var serializer = new XmlSerializer(typeof(T));
        return (T)serializer.Deserialize(new StringReader(xml));
    }
}