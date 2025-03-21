// Useful article: https://sqlpey.com/dotnet/resolve-file-upload-issues-in-minimal-api-for-net/
// Useful article: https://dev.to/leandroveiga/securing-apis-with-yarp-authentication-and-authorization-in-net-8-minimal-apis-2960

using Microsoft.AspNetCore.Antiforgery;
using Microsoft.EntityFrameworkCore;
using API.Data;
using API.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAntiforgery(options => { options.HeaderName = "X-XSRF-TOKEN"; });
//builder.Services.AddAuthentication().AddJwtBearer();
//builder.Services.AddAuthorization();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<SoilDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Create an app from the build container and configure HTTP request pipeline.
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.Use(async (context, next) =>
{
    try
    {
        await next.Invoke();
    }
    catch (AntiforgeryValidationException)
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync("Invalid antiforgery token.");
    }
});
app.UseAntiforgery();
app.UseHttpsRedirection();


/*app.UseExceptionHandler(exceptionHandlerApp
    => exceptionHandlerApp.Run(async context
        => await Results.Problem()
                     .ExecuteAsync(context)));
*/

// Endpoint: Get antiforgery token.
app.MapGet("antiforgery/token", (IAntiforgery forgeryService, HttpContext context) =>
{
    var tokens = forgeryService.GetAndStoreTokens(context);
    var xsrfToken = tokens.RequestToken!;
    return TypedResults.Content(xsrfToken, "text/plain");
});

///////////////////////////////////////////////////////////////////////////
// Endpoints
///////////////////////////////////////////////////////////////////////////

// Endpoint: Add (or update) soils.
app.MapPost("/xml/add", (SoilDbContext context, HttpRequest request)
    => Soil.Add(context, request.ToXML().ToSoils()));

// Endpoint: Search for soils and return matching full names.
app.MapGet("/xml/search", (SoilDbContext context, string name = null, string folder = null, string soilType = null, string country = null,
                           double latitude = double.NaN, double longitude = double.NaN, double radius = double.NaN,
                           string fullName = null,
                           string cropName = null, Values thickness = null, Values cll = null, Values pawc = null,
                           int numToReturn = 0)
    => Soil.Search(context, name, folder, soilType, country, latitude, longitude, radius,
                   fullName, cropName, thickness?.Doubles, cll?.Doubles, pawc?.Doubles, numToReturn).ToTextResult());

// Endpoint: Get XML for soils as specified by full names.
app.MapGet("/xml/get", (SoilDbContext context, Values fullNames)
    => Soil.Get(context, fullNames.Strings).ToFolder().ToXMLResult());

// Endpoint: Get info about a soil.
app.MapGet("/xml/info", (SoilDbContext context, string fullName)
    => Soil.Get(context, Soil.Search(context, fullName: fullName)).First()?.ToInfo().ToXMLResult());

app.Run("http://*:80");
