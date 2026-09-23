import dayjs from "dayjs"

export const ReporteCPERequest = {
  fechaDesde: dayjs(new Date()),
  fechaHasta: dayjs(new Date()),
  compradores: [],
  vendedores: [],
  productos: [],
  destinos: [],
  centros: [],
  tipoDeReporte: 0,
  estadoDeCupoEnSTOP: 0
}