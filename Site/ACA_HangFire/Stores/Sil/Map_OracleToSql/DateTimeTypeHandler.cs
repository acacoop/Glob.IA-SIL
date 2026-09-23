using System.Data;
using System.Globalization;
using Dapper;

namespace HangFire.Stores.Sil.Map_OracleToSql
{
  public class DateTimeTypeHandler : SqlMapper.TypeHandler<DateTime>
  {
    public ILogger Logger { get; }

    public DateTimeTypeHandler(ILogger logger) 
    {
      Logger = logger;
    }  

    public override DateTime Parse(object value)
    {      
      string val = value.ToString();
      if (!string.IsNullOrEmpty(val))
      {
        DateTime date = DateTime.ParseExact(val, "dd/MM/yyyy", CultureInfo.CreateSpecificCulture("es-AR"));
        return date;
      }
      return DateTime.MinValue;
    }

    public override void SetValue(IDbDataParameter parameter, DateTime value)
    {
      throw new NotImplementedException();
    }
  }
}
