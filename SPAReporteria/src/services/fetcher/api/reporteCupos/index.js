import { getApiUrlReporteria, getApiUrlReporteriaLocal } from "config";

export const postReporteCupos = async (payload) => {
  const response = await fetch(
    getApiUrlReporteria(`/api/Cupos/GetCuposParaInforme`),
    {
      method: "POST",
      headers: {
        "Content-type": "application/json",
      },
      body: JSON.stringify(payload),
    }
  );
  const data = await response.json();
  return data;
};
