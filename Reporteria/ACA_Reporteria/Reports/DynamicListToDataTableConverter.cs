using Castle.Core.Internal;
using System.Data;
using System.Dynamic;

namespace Reporteria.Reports
{
  public static class DynamicListToDataTableConverter
  {
    public static DataTable ToDataTable(List<dynamic> items, ReportInfo reportInfo)
    {
      DataTable dataTable = new DataTable();
      if (!items.IsNullOrEmpty()) 
      {
        foreach (PropertyInfo prop in reportInfo.properties)
        {
          //dataTable.Columns.Add(prop.title);  /*modificar codigo de gaspar de genear excel*/
          if (!string.IsNullOrEmpty(prop.type) && prop.type.Equals("Date"))
          {
            dataTable.Columns.Add(prop.property, typeof(DateTime));
          }
          else 
          {
            dataTable.Columns.Add(prop.property);
          }
        }
        int numberfColumn = reportInfo.properties.Count;
        foreach (ExpandoObject item in items)
        {
          var values = new object[numberfColumn];
          for (int i = 0; i < numberfColumn; i++)
          {
            //values[i] = FirstOrDefault<object>(item, propertyNames[i]);
            //si el property tiene functio la aplico al elemento....
            if (!string.IsNullOrEmpty(reportInfo.properties.ElementAt(i).selectStatement))
            {
              //modificar esto cuando agreguemos la func
              if (reportInfo.properties.ElementAt(i).property.Equals("EstadoSTOP"))
              {
                var valor = FirstOrDefault<object>(item, reportInfo.properties.ElementAt(i).property);
                string texto;
                switch (valor)
                {
                  case "0":
                    texto = "Sin CTG"; break;
                  case "1":
                    texto = "Activado"; break;
                  case "2":
                    texto = "Arribado"; break;
                  case "3":
                    texto = "Descargado"; break;
                  case "4":
                    texto = "Desviado"; break;
                  case "5":
                    texto = "Rechazado"; break;
                  case "6":
                    texto = "Anulado"; break;
                  default:
                    texto = ""; break;
                }
                values[i] = texto;
              }
              if (reportInfo.properties.ElementAt(i).property.Equals("EstadoSIL"))
              {
                var valor = FirstOrDefault<object>(item, reportInfo.properties.ElementAt(i).property);
                string texto;
                switch (valor.ToString())
                {
                  case "0":
                    texto = "Pendiente de Distribuir"; break;
                  case "1":
                    texto = ""; break;
                  case "2":
                    texto = ""; break;
                  case "3":
                    texto = "Anulado"; break;
                  case "4":
                    texto = "Distribuido"; break;
                  case "5":
                    texto = "Distribuido"; break;
                  default:
                    texto = ""; break;
                }
                values[i] = texto;
              }
            }
            else 
            {
              values[i] = FirstOrDefault<object>(item, reportInfo.properties.ElementAt(i).property);
            }
          }
          dataTable.Rows.Add(values);
        }
        //put a breakpoint here and check datatable
      }
      return dataTable;
    }

    public static T FirstOrDefault<T>( ExpandoObject eo, string key)
    {
      object r = eo.FirstOrDefault(x => x.Key == key).Value;
      return (r is T) ? (T)r : default(T);
    }

  }
}
