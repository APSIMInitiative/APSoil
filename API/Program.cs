// Useful article: https://sqlpey.com/dotnet/resolve-file-upload-issues-in-minimal-api-for-net/
// Useful article: https://dev.to/leandroveiga/securing-apis-with-yarp-authentication-and-authorization-in-net-8-minimal-apis-2960

using Microsoft.AspNetCore.Antiforgery;
using Microsoft.EntityFrameworkCore;
using API.Data;
using API.Services;
using static API.Services.SoilsFromDb;

internal class Program
{
    private static void Main(string[] args)
    {
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

        app.MapGet("/xml/search", (SoilDbContext context, string name = null, string folder = null, string soilType = null, string country = null,
                                   double latitude = double.NaN, double longitude = double.NaN, double radius = double.NaN,
                                   string fullName = null,
                                   string cropName = null, Values thickness = null, Values cll = null, bool cllIsGrav = false, Values pawc = null,
                                   int numToReturn = 0,
                                   OutputFormatEnum output = OutputFormatEnum.Names) =>
            Soil.Search(context, name, folder, soilType, country, latitude, longitude, radius,
                        fullName, cropName, thickness?.Doubles, cll?.Doubles, cllIsGrav, pawc?.Doubles, numToReturn)
                .ToXMLResult(output));

        // Endpoint: Get graph of soil.
        app.MapGet("/xml/graph", (SoilDbContext context, string fullName, Values thickness = null, Values sw = null, bool swIsGrav = false) =>
        {
            return Soil.Search(context, fullName:fullName)
                       .ToSoils()
                      ?.First()
                       .ToGraphPng(thickness?.Doubles, sw?.Doubles, swIsGrav)
                       .ToImageResult();
        });

        // Endpoint: Calculate and return the PAWC of a specified soil and crop (mm). Crop can be null.
        app.MapGet("/xml/pawc", (SoilDbContext context, string fullName, string cropName = null)
            => Soil.PAWC(context, fullName, cropName));

        // Endpoint: Calculate and return the PAW of a specified soil, crop and water content (mm). Crop can be null.
        app.MapGet("/xml/paw", (SoilDbContext context, string fullName, Values thickness, Values sw, bool swIsGrav, string cropName = null)
            => Soil.PAW(context, fullName, cropName, thickness?.Doubles, sw?.Doubles, swIsGrav));

        app.Run("http://*:80");
    }
}
