namespace SILData.Model.SolicitudTurno
{
    /// <summary>
    /// Desglose de cupos asociados a una solicitud. Devuelto por el endpoint
    /// bulk <c>POST Matches</c> para que el front vea si la solicitud ya tiene
    /// cupos parciales asignados.
    /// </summary>
    public class CuposAsociadosDesglose
    {
        /// <summary>Cantidad total de filas en SOLTURNOS para esta solicitud (parent + splits).</summary>
        public int Total { get; set; }

        /// <summary>Filas en estado Otorgado (cupo ya asignado).</summary>
        public int Otorgados { get; set; }

        /// <summary>Filas en estado Pendiente (aún no procesadas).</summary>
        public int Pendientes { get; set; }

        /// <summary>Filas en estado Rechazado.</summary>
        public int Rechazados { get; set; }
    }
}