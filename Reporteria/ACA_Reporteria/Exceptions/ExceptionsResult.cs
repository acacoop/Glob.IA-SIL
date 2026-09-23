namespace Reporteria.Exceptions
{
  public class ExceptionsResult<T>
  {
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Error { get; set; }
    public List<string> Warnings { get; set; } = new();
  }
}
