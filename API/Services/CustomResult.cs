namespace API.Services;

/// <summary>A class that represents an XML result.</summary>
/// <remarks>
/// https://andrewlock.net/returning-xml-from-minimal-apis-in-dotnet-6/
/// </remarks>
public class CustomResult<T> : IResult
{
    private string contentType;
    public T payload { get; }

    /// <summary>
    /// Creates a new instance of <see cref="CustomResult"/>.
    /// </summary>
    /// <param name="text">The text to return.</param>
    /// <param name="contentType">The content type.</param>
    public CustomResult(T payload, string contentType)
    {
        this.contentType = contentType;
        this.payload = payload;
    }

    public async Task ExecuteAsync(HttpContext httpContext)
    {
        httpContext.Response.ContentType = contentType;
        using var ms = new MemoryStream();
        using var writer = new StreamWriter(ms);
        if (payload is string)
        {
            writer.Write(payload);
            writer.Flush();
        }
        else if (payload is byte[] bytes)
            ms.Write(bytes, 0, bytes.Length);
        ms.Position = 0;
        await ms.CopyToAsync(httpContext.Response.Body);
    }
}