using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.ClassShared
{
  public class Enum
  {
    public enum TypeOfBusinessRule : int
    {
      Permisos = 100,
      InconsistenciaDeDatos = 200,
      DatosRepetidos = 300,
      DatosNoExistente = 400
    }
  }
}
