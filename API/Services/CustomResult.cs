namespace API.Services;

/// <summary>A class that represents an XML result.</summary>
/// <remarks>
/// https://andrewlock.net/returning-xml-from-minimal-apis-in-dotnet-6/
/// </remarks>
public class CustomResult : IResult
{
    private string contentType;
    // The object to serialize
    public string Text { get; }

    /// <summary>
    /// Creates a new instance of <see cref="CustomResult"/>.
    /// </summary>
    /// <param name="text">The text to return.</param>
    /// <param name="contentType">The content type.</param>
    public CustomResult(string text, string contentType)
    {
        this.contentType = contentType;
        Text = text;
    }

    public async Task ExecuteAsync(HttpContext httpContext)
    {
        httpContext.Response.ContentType = contentType;
        var ms = new MemoryStream();
        var writer = new StreamWriter(ms);
        writer.Write(Text);
        writer.Flush();
        ms.Position = 0;
        await ms.CopyToAsync(httpContext.Response.Body);
    }
}