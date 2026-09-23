using Microsoft.IdentityModel.Tokens;
using Reporteria.DataAccess;
using Reporteria.Services;
using System.Globalization;


var builder = WebApplication.CreateBuilder(args);
var cultureInfo = new CultureInfo("es-AR");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddTransient<IReportStore, CupoStore>();
builder.Services.AddTransient<IServicesReport, CuposService>();

// Application Insights: leer Connection String desde env var (recomendado Microsoft).
// Configurar en Azure App Service -> Configuration -> Application Settings:
//   APPLICATIONINSIGHTS_CONNECTION_STRING = InstrumentationKey=...;IngestionEndpoint=...
builder.Services.AddApplicationInsightsTelemetry(options =>
{
  // Desactivar adaptive sampling para no perder logs en debugging.
  // Reactivar en produccion estable si hay mucho volumen.
  options.EnableAdaptiveSampling = false;
});

// Configure logging for App Service and Console-------------------------------------
builder.Logging.ClearProviders();
builder.Logging.AddConsole();              // Log en consola local
builder.Logging.AddDebug();                // Log en depuraci�n (Visual Studio)
builder.Logging.AddAzureWebAppDiagnostics(); // Log en Azure Log Stream
builder.Logging.AddApplicationInsights();  // Manda ILogger.* a App Insights (traces custom)

// Opcional: configurar l�mites de tama�o de archivo y retenci�n (solo para App Service)
builder.Services.Configure<Microsoft.Extensions.Logging.AzureAppServices.AzureFileLoggerOptions>(options =>
{
  options.FileName = "diagnostics-";
  options.FileSizeLimit = 50 * 1024; // 50 KB
  options.RetainedFileCountLimit = 5;
});

//---------------------------------------------------------------------------------------------------



// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication("Bearer")
            .AddJwtBearer("Bearer", options =>
            {
              options.Authority = "https://silappauth.azurewebsites.net";
              options.TokenValidationParameters = new TokenValidationParameters
              {
                ValidateAudience = false
              };
            });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
  app.UseDeveloperExceptionPage();
}

// global cors policy
app.UseCors(x => x
    .AllowAnyMethod()
    .AllowAnyHeader()
    .SetIsOriginAllowed(origin => true) // allow any origin
    .WithExposedHeaders("X-Warnings", "Content-Disposition")
    .AllowCredentials()); // allow credentials

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
