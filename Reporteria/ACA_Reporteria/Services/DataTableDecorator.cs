using Reporteria.Reports;
using System.Data;

namespace Reporteria.Services
{
  public static class DataTableDecorator
  {
    public static DataTable AdjustDataTableFromReport(this DataTable dt, Report report)
    {
      foreach (DataColumn column in dt.Columns)
      {
        var propertyInfo = report.GetPropertyInfo(column.ColumnName);
        if (!string.IsNullOrEmpty(propertyInfo.title))
        {
          column.ColumnName = propertyInfo.title;
        }
      }
      return dt;
    }
  }
}
