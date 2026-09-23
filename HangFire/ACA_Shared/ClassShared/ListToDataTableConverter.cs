using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Shared.ClassShared
{
  public static class ListToDataTableConverter
  {
    public static DataTable ToDataTable<T>()
    {
      DataTable dataTable = new DataTable(typeof(T).Name);
      PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
      foreach (PropertyInfo prop in Props)
      {
        dataTable.Columns.Add(new DataColumn(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType));
      }
      return dataTable;
    }
  }
}
