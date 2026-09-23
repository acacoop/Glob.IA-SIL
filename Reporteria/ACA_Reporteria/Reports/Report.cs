using DocumentFormat.OpenXml.Office2010.Excel;
using System.Text;

namespace Reporteria.Reports
{
  public class Report
  {
    public string type { get; set; }
    public string format { get; set; }
    public string name { get; set; }
    public ReportInfo report { get; set; }

    public string BuildQuery()
    {
      StringBuilder stringBuilder = new StringBuilder("SELECT ");
      if (report.properties.Count > 0)
      {
        string selectStatement = "";
        if (string.IsNullOrEmpty(report.properties.ElementAt(0).selectStatement))
        {
          selectStatement = report.properties.ElementAt(0).property;
        }
        else
        {
          selectStatement = report.properties.ElementAt(0).selectStatement;
        }
        stringBuilder.Append(selectStatement);
        for (int i = 1; i < report.properties.Count; i++)
        {
          if (string.IsNullOrEmpty(report.properties.ElementAt(i).selectStatement))
          {
            selectStatement = report.properties.ElementAt(i).property;
          }
          else
          {
            selectStatement = report.properties.ElementAt(i).selectStatement;
          }
          stringBuilder.Append($" ,{selectStatement}");
        }
      }
      else
      {
        stringBuilder.Append('*');
      }
      return stringBuilder.ToString();
    }

    public PropertyInfo GetPropertyInfo(string columnName)
    {
      return report.properties.Where(x => x.property == columnName).First();
    }
  }

  public class ReportInfo
  {
    public bool whereStatementConditional { get; set; }
    public IList<PropertyInfo> properties { get; set; }
  }

  public class PropertyInfo
  {
    public string property { get; set; }
    public string selectStatement { get; set; }
    public string title { get; set; }
    public string type { get; set; }
    //public Func<dynamic,dynamic> function { get; set; }
  }
}
