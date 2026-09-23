using Newtonsoft.Json;

namespace Comunicacion.DataRequest
{
  public class FilterInfo
  {
    [JsonProperty(PropertyName = "FieldName")]
    public string FieldName { get; set; }

    [JsonProperty(PropertyName = "value")]
    public string Value { get; set; }
  }
}
