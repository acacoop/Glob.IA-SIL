using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.ClassShared.Types
{
  public class TablesResponse
  {
    /// <summary>
    /// Lista de valores para el filtro indicado.
    /// Varia segun los parametros de paginacion
    /// </summary>
    public IEnumerable<object>? Values { get; set; }
    /// <summary>
    /// Cantidad total de registros para el filtro indicado.
    /// Sin tener en cuenta paginacion.
    /// </summary>
    public long RecordTotal { get; set; }
  }
}
