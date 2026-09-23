using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NuGet.Common;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;

namespace Shared.ClassShared.WebServiceRequest
{
  public abstract class WebService : IDisposable
  {
    public abstract string GetWebSerive { get; set; }

    private HttpRequestMessage Request { get; set; }

    public WebService()
    {
    }

    public WebService(HttpRequestMessage Request)
    {
      this.Request = Request;
    }

    public string GetPath(string Controller)
    {
      return GetWebSerive + Controller + "/";
    }

    public HttpClient BuildHttpClient(int minutesTimeout, string token)
    {
      HttpClient httpClient = new HttpClient();
      httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
      if (minutesTimeout > 0) httpClient.Timeout = TimeSpan.FromMinutes(minutesTimeout);
      return httpClient;
    }

    public async Task<T> RequestPostAndDeserializeAsync<T>(string Controller, string Action, object Data, string token , int minutesTimeOut = 0 )
    {
      HttpClient client = BuildHttpClient(minutesTimeOut, token);
      string jsonString = JsonConvert.SerializeObject(Data);
      HttpResponseMessage json = await client.PostAsync(requestUri: GetPath(Controller) + Action, content: new StringContent(jsonString, Encoding.UTF8, "application/json"));
      if (json.StatusCode == HttpStatusCode.NotFound)
      {
        throw new Exception("No se encontro el action en el resource server.");
      }

      if (json.StatusCode == HttpStatusCode.InternalServerError || json.StatusCode == HttpStatusCode.Conflict)
      {
        throw new Exception("Error al intentar consultar la API de SILData");
      }

      string content = await json.Content.ReadAsStringAsync();

      if (string.IsNullOrWhiteSpace(content))
      {
        Console.WriteLine($"[{DateTime.Now}] Respuesta vacía del endpoint {GetPath(Controller) + Action}");
        return Activator.CreateInstance<T>(); // devuelve una instancia vacía en vez de null
      }

      try
      {
        var result = Deserialize<T>(content);
        if (result == null)
        {
          Console.WriteLine($"[{DateTime.Now}] No se pudo deserializar correctamente la respuesta del endpoint {GetPath(Controller) + Action}");
          return Activator.CreateInstance<T>();
        }
        return result;
      }
      catch (Exception ex)
      {
        Console.WriteLine($"[{DateTime.Now}] Error deserializando respuesta: {ex.Message}");
        return Activator.CreateInstance<T>();
      }
      //return Deserialize<T>(await json.Content.ReadAsStringAsync());
    }

    public T Deserialize<T>(string JsonResponse)
    {
      return JsonConvert.DeserializeObject<T>(JsonResponse);
    }
    private string GetToken()
    {
      if (Request != null)
      {
        return Request.Headers.Authorization.Parameter.Replace("Bearer ", "");
      }

      return "";
    }

    public void Dispose()
    {
      GC.SuppressFinalize(this);
    }
  }
}
