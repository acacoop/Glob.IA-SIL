namespace Shared.ClassShared.BusinessExceptions
{
  public class BusinessRule
  {
    public string Code { get; set; }
    public string Message { get; set; }
    public Enum.TypeOfBusinessRule TypeOfBusinessRule { get; set; }
  }
}
