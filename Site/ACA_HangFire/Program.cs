using Hangfire;
using Hangfire.Dashboard;
using Hangfire.Extensions;
using HangFire.Services;
using HangFire.Stores.Own;
using HangFire.Stores.Sil;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);
var cultureInfo = new CultureInfo("es-AR");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddApplicationInsightsTelemetry();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.ConfigureHangFire(builder.Configuration);

builder.Services.AddTransient<IOwnCupoStore, HangFire.Stores.Own.CupoStore>();
builder.Services.AddTransient<ISilCupoStore, HangFire.Stores.Sil.CupoStore>();
builder.Services.AddTransient<ICuposService, ImportCuposService>();

builder.Services
    .AddAuthentication(cfg =>
    {
      cfg.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
      cfg.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie()
    .AddOpenIdConnect(cfg =>
    {
      cfg.Authority = "https://silappauth.azurewebsites.net";
      cfg.ClientId = "d6WSXvV2jWeiy2wPXvJP37nffXWUukh9";
      cfg.ClientSecret = builder.Configuration["ClientSecret"] ?? throw new InvalidOperationException("ClientSecret configuration is required.");
      cfg.ResponseType = "code";
      cfg.UsePkce = false;

      cfg.Scope.Clear();
      cfg.Scope.Add("openid");
      cfg.Scope.Add("profile");
      cfg.Scope.Add("datos");
      cfg.Scope.Add("offline_access");
    });

var app = builder.Build();
app.UseRouting();

app.MapHangfireDashboard();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Use(async (context, next) =>
{
  if (context.Request.Path.Equals("/hangfire", StringComparison.OrdinalIgnoreCase)
      && (context.User.Identity is null || !context.User.Identity.IsAuthenticated))
  {
    await context.ChallengeAsync();
    return;
  }

  await next();
});

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
  Authorization = new IDashboardAuthorizationFilter[]
    {
        new MyAuthorizationFilter()
    }
});


var backgroundJobClient = app.Services.GetRequiredService<IBackgroundJobClient>();

//backgroundJobClient.Enqueue(() =>
//  new ImportCuposService(app.Services.GetRequiredService<IOwnCupoStore>(), app.Services.GetRequiredService<ISilCupoStore>(), app.Services.GetRequiredService<ILogger>()).ImportCupos()
//);

//RecurringJob.AddOrUpdate("Carga inicial de Cupos", () => app.Services.GetRequiredService<ICuposService>().ImportCupos(), "*/15 * * * *");
RecurringJob.AddOrUpdate("Carga Diaria de Cupos", () => app.Services.GetRequiredService<ICuposService>().ImportCupos(), "0 7 * * *");

app.Run();
