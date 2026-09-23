using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Shared.ClassShared.BusinessExceptions;

namespace Shared.ClassShared
{
  public class ExceptionFilter : IActionFilter, IOrderedFilter
  {
    public int Order => int.MaxValue - 10;

    public void OnActionExecuted(ActionExecutedContext context)
    {
      if (context.Exception != null) 
      {
        if (context.Exception is BusinessException businessException)
        {
          context.Result = new ObjectResult(new ExceptionMessageContent()
          {
            Error = businessException.ruleFail.Code,
            Message = businessException.ruleFail.Message,
            TypeOfBusinessRule = (int)businessException.ruleFail.TypeOfBusinessRule,
          })
          { };
          context.ExceptionHandled = true;
        }
        else
        {
          //ErrorLog.Write(context.Exception);
          context.Result = new ObjectResult(new ExceptionMessageContent()
          {
            Error = "InternalError",
            Message = "Ha ocurrido un error.",
          })
          { };
          context.ExceptionHandled = true;
        }
      } 
    }

    public void OnActionExecuting(ActionExecutingContext context){}
  }
}
