using Shared.StaticShared;
using SILData.DataAccess;
using SILData.Model;

namespace SILData.Services
{
  public class AccountService
  {
    private IAccountStore Store { get; set; }
    public AccountService(IAccountStore s)
    {
      this.Store = s;
    }
    public async Task<CuentaSIL> GetVendedorByCuenta(long cuenta)
    {
      return await Store.GetVendedorByCuenta(cuenta);
    }
    public async Task<CuentaSIL> GetCompradorByCuenta(long cuenta)
    {
      return await Store.GetCompradorByCuenta(cuenta);
    }
    /// <summary>
    /// Espejo batch de <see cref="GetVendedorByCuenta"/>. Una sola query
    /// con <c>IN (.., .., ..)</c> contra el catálogo para evitar el N+1
    /// cuando el matching hidrata el nombre del vendedor para N cupos.
    /// (El nombre del comprador ya viene en la fila de cuposcorre vía
    /// <c>NOMDESTINATARIO</c>, así que no hay un batch equivalente de
    /// compradores acá.)
    /// </summary>
    public async Task<IList<CuentaSIL>> GetVendedoresByCuentas(IEnumerable<long> cuentas)
    {
      return await Store.GetVendedoresByCuentas(cuentas);
    }
    public async Task<IList<CuentaSIL>> GetAll()
    {
      return await Store.GetAll();
    }
    public async Task<IList<CuentaSIL>> GetStartWith(string Text, int limit = 10)
    {
      if (StaticOperations.IsNumber(Text))
      {
        return await Store.FindStartsWithLimit(Text, limit);
      }
      else
      {
        return await Store.FindStartsWithLimit(Text.ToUpper(), limit);
      }
    }

    public async Task<IList<CuentaSIL>> GetContain(string Text, int limit = 10)
    {
      if (StaticOperations.IsNumber(Text))
      {
        return await Store.FindContainWithLimit(Text, limit);
      }
      else
      {
        return await Store.FindContainWithLimit(Text.ToUpper(), limit);
      }

    }

    public async Task<IList<CuentaSIL>> GetLike(string Text)
    {
      if (StaticOperations.IsNumber(Text))
        return await Store.FindLike(Text);
      else
        throw new InvalidOperationException();
      

    }
  }
}
