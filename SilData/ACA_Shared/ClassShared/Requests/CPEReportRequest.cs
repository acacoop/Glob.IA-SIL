using Microsoft.Extensions.ObjectPool;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Shared.ClassShared.Requests
{
  public class CPEReportRequest: ICloneable
  {
    [Required]
    public DateTime FechaDesde { get; set; }
    [Required]
    public DateTime FechaHasta { get; set; }
    [Required]
    public int TipoDeReporte { get; set; }
    public IList<string>? Compradores { get; set; }
    public IList<string>? Vendedores { get; set; }
    public IList<string>? Productos { get; set; }
    public IList<string>? Centros { get; set; }
    public IList<string>? Destinos { get; set; }
    public int EstadoDeCupoEnSTOP { get; set; }

    public object Clone()
    {
      return new CPEReportRequest() { 
        FechaDesde = this.FechaDesde,
        FechaHasta = this.FechaHasta, 
        TipoDeReporte = this.TipoDeReporte,
        Compradores = this.Compradores,
        Vendedores = this.Vendedores, 
        Productos = this.Productos, 
        Centros = this.Centros, 
        Destinos = this.Destinos, 
        EstadoDeCupoEnSTOP = this.EstadoDeCupoEnSTOP
      };
    }

  }
}
