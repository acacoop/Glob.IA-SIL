using System.ComponentModel.DataAnnotations;

namespace Shared.ClassShared.Types
{
  public class TablesRequest
  {
    [Required]
    public int PageSize { get; set; }
    [Required]
    public int PageNumber { get; set; }
    [Required]
    public string OrderColumn { get; set; }
    [Required]
    public bool OrderDesc { get; set; }
    /*Filter option 1*/
    public List<FilterRequest>? Filters { get; set; }
    /*Filter option 2*/
    public object? ParameterClass { get; set; }
  }
}
