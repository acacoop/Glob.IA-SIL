import { getApiUrlReporteria, getApiUrlReporteriaLocal } from "config";
import { add, downloadExcel } from "../serviceApi";

export function getReporteCPE(request) {
  return add(getApiUrlReporteria("/api/cupos/Cupos"), request)
}

export function getReporteExcelCPE(request) {
  return downloadExcel(getApiUrlReporteria("/api/cupos/Export"), request)
}