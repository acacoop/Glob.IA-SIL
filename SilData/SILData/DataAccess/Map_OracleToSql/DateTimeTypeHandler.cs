using System.Data;
using System.Globalization;
using Dapper;
using Oracle.ManagedDataAccess.Client;

namespace SILData.DataAccess.Map_OracleToSql
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
      if (value is DateTime dateTime)
      {
        return dateTime;
      }
      string val = value.ToString();
      if (!string.IsNullOrEmpty(val))
      {
        DateTime date;
        if (DateTime.TryParseExact(
          val,
          new[] { "d/M/yyyy HH:mm:ss", "d/MM/yyyy HH:mm:ss", "dd/M/yyyy HH:mm:ss", "dd/MM/yyyy HH:mm:ss", "dd/MM/yyyy" },
          CultureInfo.CreateSpecificCulture("es-AR"),
          DateTimeStyles.None,
          out date)
          )
        {
          return date;
        }
      }
      return DateTime.MinValue;
    }

    public override void SetValue(IDbDataParameter parameter, DateTime value)
    {
      throw new NotImplementedException();
      //if (parameter is OracleParameter oracleParameter)
      //{
      //  oracleParameter.OracleDbType = OracleDbType.Date;
      //  oracleParameter.Value = value;
      //}
      //else
      //{
      //  parameter.DbType = DbType.Date;
      //  parameter.Value = value;
      //}
    }
  }
}