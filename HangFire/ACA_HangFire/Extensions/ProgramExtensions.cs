using Hangfire;
using Hangfire.SqlServer;

namespace Hangfire.Extensions
{
  public static class ProgramExtensions
  {
    public static void ConfigureHangFire(this IServiceCollection services, IConfiguration Configuration) 
    {
       services.AddHangfire(config => config
          .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
          .UseSimpleAssemblyNameTypeSerializer()
          .UseRecommendedSerializerSettings()
          .UseSqlServerStorage(Configuration.GetConnectionString("HangfireConnection"), new SqlServerStorageOptions
          {
            CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
            SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
            QueuePollInterval = TimeSpan.Zero,
            UseRecommendedIsolationLevel = true,
            DisableGlobalLocks = true
          }));
      services.AddHangfireServer();
    }
  }
}
