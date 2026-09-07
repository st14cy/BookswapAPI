using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app=builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
   //app.MapScalarApiReference(options =>
   //{
   //    options
   //        .WithTitle("BookswapAPI")           // Название API
   //        .WithTheme(ScalarTheme.Mars)        // Тема: Mars, Purple, DeepSpace, Moon и др.
   //        .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
   //        .WithPreferredScheme("Bearer")      // Авторизация по умолчанию
   //        .WithProxy(null)                    // Отключаем внешний прокси (безопаснее)
   //        .WithSidebar(true)                  // Показываем боковое меню
   //        .ForceDarkMode();                   // Принудительно темная тема
   //});
    app.MapScalarApiReference();
}

app.MapControllers();
app.Run();

