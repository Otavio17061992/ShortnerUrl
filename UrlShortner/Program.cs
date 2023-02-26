using Records;
using LiteDB;

var builder = WebApplication.CreateBuilder(args);


// you could also get it from IConfiguration interface
var connectionString = "short.db";


// add as a singleton - it's a single file with a single access point
builder.Services.AddSingleton<ILiteDatabase, LiteDatabase>(
    x => new LiteDatabase(connectionString));

// redacted
var app = builder.Build();

app.MapGet("/", async (HttpContext ctx) =>
{
    ctx.Response.Headers.ContentType = new Microsoft.Extensions.Primitives.StringValues("text/html; charset=UTF-8");
    await ctx.Response.SendFileAsync("wwwroot/index.html");
});


app.MapGet("/{chunck}", (string chunck, ILiteDatabase db) =>
    db.GetCollection<ShortUrl>().FindOne(x => x.Chunck == chunck)
    is ShortUrl url
    ? Results.Redirect(url.Url)
    : Results.NotFound());


app.MapGet("urls", (ShortUrl shortUrl, HttpContext ctx, ILiteDatabase db) =>
{
    // check if is a valid url
    if (Uri.TryCreate(shortUrl.Url, UriKind.RelativeOrAbsolute
        , out Uri? parsedUri))
    {
        // generetes a random value
        shortUrl.Chunck = Nanoid.Nanoid.Generate(size: 9);

        // inserts new rwcord in the database
        db.GetCollection<ShortUrl>(BsonAutoId.Guid).Insert(shortUrl);


        var rawShortUrl = $"{ctx.Request.Scheme}://{ctx.Request.Host}/{shortUrl.Chunck}";

        return Results.Ok(new {ShortUrl = rawShortUrl});
    }

    return Results.BadRequest(new { ErrorMessage = "Invalid Url" });
});



app.Run();
