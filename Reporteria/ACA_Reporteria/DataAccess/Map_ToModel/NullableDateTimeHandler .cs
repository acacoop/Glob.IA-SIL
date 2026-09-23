using Dapper;
using System.Data;

namespace Reporteria.DataAccess.Map_ToModel
{
  public class NullableDateTimeHandler : SqlMapper.TypeHandler<DateTime?>
  {
    public ILogger Logger { get; }

    public NullableDateTimeHandler(ILogger logger)
    {
      Logger = logger;
    }

    public override void SetValue(IDbDataParameter parameter, DateTime? value)
    {
      if (value.HasValue)
      {
        parameter.Value = value.Value;
      }
      else
      {
        parameter.Value = DBNull.Value;
      }
    }

    public override DateTime? Parse(object value)
    {
      try
      {
        if (value == null)
        {
          return null;
        }

        if (value is DateTime)
        {
          return (DateTime)value;
        }

        return Convert.ToDateTime(value);
      }
      catch (Exception e)
      {
        Logger.LogCritical("error con: " + value);
        throw;
      }
    }
  }
}
