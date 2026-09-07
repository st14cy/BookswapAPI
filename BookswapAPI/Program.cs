using BookswapAPI.Services.Book;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddHttpClient<IBookService, BookService>(client =>
{
    client.BaseAddress = new Uri("https://openlibrary.org/");
    client.DefaultRequestHeaders.Add("User-Agent", "BookswapAPI/1.0 (contact@your-email.com)");
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    
    
    app.MapGet("/", () => Results.Redirect("/scalar/v1"));
}

app.MapControllers();
app.Run();