using System.Xml.Serialization;

namespace API.Services;


/// <summary>
/// A class that represents an XML result.
/// </summary>
/// <remarks>
/// https://andrewlock.net/returning-xml-from-minimal-apis-in-dotnet-6/
/// </remarks>
public class XmlResult : IResult
{
    // Create the serializer that will actually perform the XML serialization
    private static readonly XmlSerializer Serializer = new(typeof(string));

    // The object to serialize
    private readonly string xml;

    public XmlResult(Models.Soil[] result)
    {
        xml = result.ToXML();
    }

    public async Task ExecuteAsync(HttpContext httpContext)
    {
        httpContext.Response.ContentType = "application/xml";
        var ms = new MemoryStream();
        var writer = new StreamWriter(ms);
        writer.Write(xml);
        writer.Flush();
        ms.Position = 0;
        await ms.CopyToAsync(httpContext.Response.Body);
    }
}