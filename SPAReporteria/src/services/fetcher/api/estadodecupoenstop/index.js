import get from "../serviceApi";
import { getApiUrlReporteria } from "config";

export const getAllEstadoDeCupoEnStop = async () => {
  try {
    const response = await get(
      getApiUrlReporteria("/api/datos/EstadoDeCupoEnStop")
    );
    return response ?? {};
  } catch (err) {
    return {};
  }
};
