using System.Data;
using Castle.Core.Internal;
using Dapper;

namespace HangFire.Stores.Sil.Map_OracleToSql
{
  public class BooleanTypeHandler : SqlMapper.TypeHandler<bool>
  {
    /*Get of DB*/
    public override bool Parse(object value)
    {
      string val = value.ToString();
      if (!string.IsNullOrEmpty(val))
      {
        if (val.Trim() == "S" || val.Trim() == "1")
        {
          return true;
        }
      }
      return false;
    }

    /*Set of DB*/
    public override void SetValue(IDbDataParameter parameter, bool value)
    {
      throw new NotImplementedException();
    }
  }
}
