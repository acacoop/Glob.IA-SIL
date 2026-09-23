namespace Reporteria.Model
{
  public static class DictionaryOfValues
  {
    public static readonly Dictionary<int,string> OpcionesCupos = new Dictionary<int, string>()
        {
            { 0, "Solo Registrado en SIL"},
            { 1, "Solo Registrado en STOP"},
            { 2, "Todos"},
        };

    public static readonly Dictionary<int, string> EstadoDeCupoEnStop = new Dictionary<int, string>()
        {   { -1, "Todos"},
            { 0, "Sin CTG"},
            { 1, "Activado"},
            { 2, "Arribado"},
            { 3, "Descargado"},
            { 4, "Desviado"},
            { 5, "Rechazado"},
            { 6, "Anulado"},
            
        };
  }
}
