using Hangfire.Dashboard;

namespace HangFire.Services
{
  public class MyAuthorizationFilter : IDashboardAuthorizationFilter
  {
    public bool Authorize(DashboardContext context)
    {
      // Allow all authenticated users to see the Dashboard (potentially dangerous).
      return context.GetHttpContext().User.Identity is not null && context.GetHttpContext().User.Identity.IsAuthenticated;
    }
  }
}
