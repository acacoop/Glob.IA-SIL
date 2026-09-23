import dayjs from "dayjs";

export const ReporteCuposRequest = {
  fecha: dayjs(new Date()),
  compradores: [],
  vendedores: [],
  productos: [],
  destinos: [],
  centros: [],
};
