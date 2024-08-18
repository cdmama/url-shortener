using UrlShortener.Models;
using UrlShortener.Services;

var builder = WebApplication.CreateBuilder(args);

#region Services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IUrlDatabase, UrlDatabase>();
builder.Services.AddScoped<IUrlShortenerService, UrlShortenerService>();
#endregion

var app = builder.Build();

#region Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();
#endregion

#region Endpoints
app.MapGet("/ping", () =>
{
    return "pong";
})
.WithName("ping");

app.MapPost("/encode", (HttpContext context, IUrlShortenerService service, EncodeRequest request) =>
{   
    try
    {
        var shortcode = service.GetShortCode(request.Url);

        var uriBuilder = new UriBuilder(context.Request.Scheme, context.Request.Host.Host, context.Request.Host.Port ?? 80, shortcode);

        return Results.Ok(new EncodeResponse { ShortUrl = uriBuilder.Uri.ToString() });
    }
    catch (FormatException)
    {
        // log the error
        return Results.BadRequest();
    }
    catch (Exception ex)
    {
        // log the error
        return Results.Problem();
    }

});

app.MapPost("/decode", (IUrlShortenerService service, DecodeRequest request) =>
{
    try
    {
        var fullUrl = service.GetFullUrl(request.ShortUrl);

        return Results.Ok(new DecodeResponse { Url = fullUrl });
    }
    catch (ArgumentOutOfRangeException)
    {
        // log the error
        return Results.NotFound();
    }
    catch (FormatException)
    {
        // log the error
        return Results.BadRequest();
    }
    catch (Exception ex)
    {
        // log the error
        return Results.Problem();
    }
});

app.MapGet("/{shortcode}", (IUrlShortenerService service, string shortcode) =>
{
    try
    {
        var fullUrl = service.GetFullUrlFromShortCode(shortcode);

        return Results.Redirect(fullUrl);
    }
    catch (ArgumentOutOfRangeException)
    {
        // log the error
        return Results.NotFound();
    }
    catch (FormatException)
    {
        // log the error
        return Results.BadRequest();
    }
    catch (Exception ex)
    {
        // log the error
        return Results.Problem();
    }
});
#endregion

app.Run();

public partial class Program { }