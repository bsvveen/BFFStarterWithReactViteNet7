
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;

// For production use
//var builder = WebApplication.CreateBuilder(new WebApplicationOptions
//{
//    WebRootPath = Path.Combine(Directory.GetCurrentDirectory(), "ClientApp/dist/")
//});

var builder = WebApplication.CreateBuilder();


var services = builder.Services;

services.AddControllers();
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

var ClientId = builder.Configuration["Authentication:Google:ClientId"];
var ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
services.AddSecurity(ClientId, ClientSecret);


var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    var AllowedServerPaths = new List<string> { "/api", "/swagger", "/account" };
    app.UseWhen(context => !AllowedServerPaths.Any(s => context.Request.Path.StartsWithSegments(s)), app =>
    {
        app.UseSpa(spa => { spa.UseProxyToSpaDevelopmentServer("https://localhost:3000"); });
    });
   
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSecurityHeaders();
app.MapControllers();
app.Run();