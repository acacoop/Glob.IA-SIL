import get from "../serviceApi";
import { getApiUrlData, getApiSilData } from "config";

export const getAllBuyerAccounts = () => {
  return get(getApiUrlData("/api/AccountData/Comprador"));
};

export const getBuyerAccount = (id) => {
  return get(getApiUrlData(`/api/AccountData/Comprador/${id}`));
};

export const getBuyerAccounts = (text) => {
  return get(getApiUrlData(`/api/AccountData/Comprador/${text}`));
};

export const getCompradorByFilter = async (filter) => {
  const response = await fetch(
    getApiUrlData(`/api/AccountData/Comprador/${filter}`),
    {
      method: "GET",
      headers: {
        "Content-type": "application/json",
      },
    }
  );
  const data = await response.json();
  return data;
};
