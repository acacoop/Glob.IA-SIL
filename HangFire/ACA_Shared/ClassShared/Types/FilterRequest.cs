using Shared.StaticShared.Enumerators;

namespace Shared.ClassShared.Types
{
  public class FilterRequest
  {
    public string Property { get; set; }
    public string Value { get; set; }
    public TypesOfComparison OperatorOfComparison { get; set; }

  }
}
