using System.Data;
using Castle.Core.Internal;
using Dapper;

namespace SILData.DataAccess.Map_OracleToSql
{
  public class BooleanTypeHandler : SqlMapper.TypeHandler<bool>
  {
    public override bool Parse(object value)
    {
      if (value == null || value == DBNull.Value)
        return false;

      var str = value.ToString().Trim().ToUpperInvariant();

      return str switch
      {
        "S" => true,
        "Y" => true,
        "1" => true,
        "TRUE" => true,
        "T" => true,
        _ => false
      };
    }

    public override void SetValue(IDbDataParameter parameter, bool value)
    {
      // Si tu DB espera "S"/"N" o "1"/"0", define eso acá:
      parameter.Value = value ? "1" : "0";
    }
  }
}
