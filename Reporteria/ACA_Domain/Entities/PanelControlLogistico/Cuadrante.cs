using Domain.Entities.Externo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.PanelControlLogistico
{
  public class Cuadrante
  {
    public required string CodGrano { get; set; }
    public required string NomGrano { get; set; }
    public DateTime Fecha { get; set; }
    public int CuposCount { get; set; }
    public required List<Cupo> CuposDetail { get; set; }
    //visible en columna no turneable
    public int NoSTOPCount { get; set; }
    public required List<Cupo> NoSTOPDetail { get; set; }
    public int NoSILCount { get; set; }
    public required List<Cupo> NoSILDetail { get; set; }
    public int SinCTGCount { get; set; }
    public required List<Cupo> SinCTGDetail { get; set; }
    public int ActivadosCount { get; set; }
    public required List<Cupo> ActivadosDetail { get; set; }
    public int ArribadosCount { get; set; }
    public required List<Cupo> ArribadosDetail { get; set; }
    public int DescargadosCount { get; set; }
    public required List<Cupo> DescargadosDetail { get; set; }
  }
}
