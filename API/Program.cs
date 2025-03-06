// Useful article: https://sqlpey.com/dotnet/resolve-file-upload-issues-in-minimal-api-for-net/
// Useful article: https://dev.to/leandroveiga/securing-apis-with-yarp-authentication-and-authorization-in-net-8-minimal-apis-2960

using Microsoft.AspNetCore.Antiforgery;
using Microsoft.EntityFrameworkCore;
using API.Data;

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

// Ensure the database is created and apply migrations
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<SoilDbContext>();
    dbContext.Database.EnsureCreated();
}

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
app.MapPost("/add", API.Services.Soil.Add);

// Endpoint: Import soils from old APSoil format.
app.MapPost("/importold", API.Services.Soil.AddOld);

// Endpoint: Get soils
app.MapGet("/get", API.Services.Soil.Get);

app.Run();
