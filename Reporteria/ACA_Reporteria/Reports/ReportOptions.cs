namespace Reporteria.Reports
{
  public class ReportOptions
  {
    public IList<ColumnOption<dynamic>> Options { get; set; }
  }

  public class ColumnOption<T>
  {
    public string Name { get; set; }
    public T Type { get; set; }
    public Func<object, object> Value { get; set; }
  }
}
