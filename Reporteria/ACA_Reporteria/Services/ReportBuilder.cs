using Newtonsoft.Json;
using Reporteria.Reports;

namespace Reporteria.Services
{
  public static class ReportBuilder
  {
    public static Report GetReport(string reportName)
    {
      using (StreamReader r = new StreamReader(reportName))
      {
        string json = r.ReadToEnd();
        if (json == null) throw new Exception("Reporte no encontrado");
        Report items = JsonConvert.DeserializeObject<Report>(json);
        return items;
      }
    }
  }
}
