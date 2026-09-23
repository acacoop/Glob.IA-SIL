import get from "../serviceApi";
import { getApiUrlReporteria } from "config";

export const getAllTiposDeReporte = async () => {
  try {
    const response = await get(getApiUrlReporteria("/api/datos/OpcionesCupos"));
    return response ?? {};
  } catch (err) {
    return {};
  }
};
