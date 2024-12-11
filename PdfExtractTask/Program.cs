using Microsoft.OpenApi.Models;
using PdfExtractTask.Filters;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "PDF Keyword API",
        Version = "v1",
        Description = "An API to process ZIP files containing PDFs and search for keywords."
    });

    // Enable file upload support
    options.OperationFilter<SwaggerFileOperationFilter>();
});
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "PDF Keyword API V1");
        options.RoutePrefix = string.Empty; // Serves Swagger UI at the app's root
    });
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
