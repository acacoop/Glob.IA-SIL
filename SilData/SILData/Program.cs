using Dapper;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Shared.ClassShared.Interfaces;
using SILData.DataAccess;
using SILData.DataAccess.Map_OracleToSql;
using SILData.Model.Mapping;
using SILData.Services;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);
var cultureInfo = new CultureInfo("es-AR");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;


//SqlMapper.AddTypeHandler(new BooleanTypeHandler());
//SqlMapper.AddTypeHandler(new DateTimeTypeHandler(builder.Logging.CreateLogger<DateTimeTypeHandler>()));

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddTransient<ISILCuposStore, CuposStore>();
builder.Services.AddTransient<ISILCuposServices, CuposServices>();

builder.Services.AddScoped<ICuposDisponiblesStore, CuposDisponiblesStore>();
builder.Services.AddScoped<ICuposDisponiblesService, CuposDisponiblesService>();

builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddScoped<ISolicitudTurnoService, SolicitudTurnoService>();
builder.Services.AddScoped<ISolicitudTurnoStore, SolicitudTurnoStore>();

// Servicio del flujo V2 de matching para Distribución. Aislado de
// SolicitudTurnoService para no modificarlo. Usa el INNER JOIN nativo de
// ISolicitudTurnoStore.GetCuposConMatchesAsync.
builder.Services.AddScoped<SolicitudTurnoMatchingV2Service>();

// Motor de Matching: clasifica pares (cupo, solicitud) como Directo / Parcial / Condicional.
builder.Services.AddSingleton<ACA.Matching.Engine.IMatchingEngine, ACA.Matching.Engine.MatchingEngine>();

// Resolver de pertenencia cupo → zonas geográficas (JOIN PUERTOPORZONA + ZONASGEOGRAFICAS).
builder.Services.AddScoped<SILData.Services.IZonaGeograficaResolver, SILData.Services.ZonaGeograficaResolver>();

builder.Services.AddScoped<IGeographicalAereaService, GeographicalAereaService>();
builder.Services.AddScoped<IGeographicalAereaStore, GeographicalAereaStore>();

builder.Services.AddTransient<IAccountStore, VendedorStore>();
builder.Services.AddTransient<AccountService>();


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
  options.AddSecurityDefinition(name: "Bearer", securityScheme: new OpenApiSecurityScheme
  {
    Name = "Authorization",
    Description = "Enter the Bearer Authorization string as following: `Bearer Generated-JWT-Token...`",
    In = ParameterLocation.Header,
    Type = SecuritySchemeType.ApiKey,
    Scheme = "Bearer"
  });
  options.AddSecurityRequirement(new OpenApiSecurityRequirement
{
    {
        new OpenApiSecurityScheme
        {
            Name = "Bearer",
            In = ParameterLocation.Header,
            Reference = new OpenApiReference
            {
                Id = "Bearer",
                Type = ReferenceType.SecurityScheme
            }
        },
        new List<string>()
    }
});
});

builder.Services.AddAuthentication("Bearer")
            .AddJwtBearer("Bearer", options =>
            {
              options.Authority = builder.Configuration.GetSection("Auth").GetValue<string>("Authority");

              options.TokenValidationParameters = new TokenValidationParameters
              {
                ValidateAudience = false,
                ValidateIssuer = false,
              };
            });
builder.Services.AddScoped<IDataService, DataService>();
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

// global cors policy
app.UseCors(x => x
    .AllowAnyMethod()
    .AllowAnyHeader()
    .SetIsOriginAllowed(origin => true) // allow any origin
    .AllowCredentials()); // allow credentials

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();
