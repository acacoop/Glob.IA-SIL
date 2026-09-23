using SILData.Model;

namespace SILData.DataAccess
{
  public interface IAccountStore
  {
    Task<IList<CuentaSIL>> GetAll();
    Task<IList<CuentaSIL>> GetById();
    Task<CuentaSIL> GetVendedorByCuenta(long cuenta);
    Task<CuentaSIL> GetCompradorByCuenta(long cuenta);
    /// <summary>
    /// Lookup batch de múltiples vendedores por sus cuentas. Devuelve los
    /// que existan en la tabla maestra; las cuentas desconocidas quedan
    /// simplemente fuera del resultado. Pensado para evitar el N+1 cuando
    /// el motor de matching hidrata el nombre del vendedor de N cupos
    /// (el nombre del comprador ya viene en la fila de cuposcorre, por
    /// lo que no se necesita un batch de compradores acá).
    /// </summary>
    Task<IList<CuentaSIL>> GetVendedoresByCuentas(IEnumerable<long> cuentas);
    Task<IList<CuentaSIL>> FindStartsWithLimit(string filtro, int limit);
    Task<IList<CuentaSIL>> FindContainWithLimit(string filtro, int limit);
    Task<IList<CuentaSIL>> FindLike(string filtro);
  }
}