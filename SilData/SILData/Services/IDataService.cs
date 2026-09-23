using SILData.Model;

namespace SILData.Services
{
  public interface IDataService
  {
    Task<IEnumerable<ProductoSIL>> FindProductoStartsWithLimit(string filtro, int limit);
    Task<IEnumerable<ProductoSIL>> GetProductos();
    Task<ProductoSIL?> GetProducto(string id);
    Task<IEnumerable<ProductoSIL>> GetProductos(IList<string> ids);
    Task<IEnumerable<CentroSIL>> GetCentros();
    Task<IEnumerable<CentroSIL>> GetCentros(IList<string> ids);
    Task<IEnumerable<DTablaSIL>> GetMonedas();
    Task<IEnumerable<DTablaSIL>> GetMonedas(IList<string> ids);
    Task<IEnumerable<DTablaSIL>> GetCondicionesMercaderia();
    Task<IEnumerable<DTablaSIL>> GetCondicionesMercaderia(IList<string> ids);
    Task<IEnumerable<DTablaSIL>> GetTiposCuenta();
    Task<IEnumerable<DTablaSIL>> GetTiposCuenta(IList<string> ids);
    Task<IEnumerable<DTablaSIL>> GetTiposOperacion();
    Task<IEnumerable<DTablaSIL>> GetTiposOperacion(IList<string> ids);
    Task<IEnumerable<TbCoperSIL>> GetZonaComercial();
    Task<IEnumerable<TbCoperSIL>> GetZonaComercial(IList<string> ids);

  }
}
