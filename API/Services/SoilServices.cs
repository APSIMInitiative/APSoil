using SoilAPI.Data;
using SoilAPI.Models;

public class SoilServices
{
    // https://andrewhalil.com/2024/07/25/how-to-unit-test-a-net-core-minimal-web-api/

    public static async Task<Microsoft.AspNetCore.Http.IResult> Upload(SoilDbContext context, Soil soil)
    {
        try
        {
            if (soil == null)
            {
                return Results.BadRequest("Soil record is null.");
            }
            return Results.Ok();
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.ToString());
        }
    }
}