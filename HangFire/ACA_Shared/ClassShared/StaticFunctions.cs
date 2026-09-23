using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Shared.ClassShared
{
  public static class StaticFunctions
  {
    public static DataTable ToDataTable<T>(List<T> items)
    {
      DataTable dataTable = new DataTable(typeof(T).Name);
      //Get all the properties
      PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
      foreach (PropertyInfo prop in Props)
      {
        //Setting column names as Property names
        dataTable.Columns.Add(prop.Name, prop.GetType());
        //dataTable.Columns.fi
      }
      foreach (T item in items)
      {
        var values = new object[Props.Length];
        for (int i = 0; i < Props.Length; i++)
        {

          if (Props[i].GetValue(item, null) != null)
          {
            if (Props[i].PropertyType == typeof(string))
            {
              int maxLength = item.GetAttributeFrom<MaxLengthAttribute>(Props[i].Name).Length;
              if (Props[i].GetValue(item, null).ToString().Length > maxLength)
              {
                values[i] = Props[i].GetValue(item, null).ToString().Substring(0, maxLength);
              }
              else 
              {
                values[i] = Props[i].GetValue(item, null).ToString();
              }
            }
            else
            {
              values[i] = Props[i].GetValue(item, null);
            }
          }
          else 
          {
            values[i] = null;
          }
        }
        dataTable.Rows.Add(values);
      }
      return dataTable;
    }

    public static T GetAttributeFrom<T>(this object instance, string propertyName) where T : Attribute
    {
      var attrType = typeof(T);
      var property = instance.GetType().GetProperty(propertyName);
      return (T)property.GetCustomAttributes(attrType, false).First();
    }

    
  }
}
