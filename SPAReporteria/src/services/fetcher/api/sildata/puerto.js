import get from "../serviceApi";
import { getApiUrlData, getApiSilData } from "config";

export const getAllPuertoAccounts = () => {
  return get(getApiUrlData("/api/AccountData/Puerto"));
};

export const getPuertoAccount = (id) => {
  return get(getApiUrlData(`/api/AccountData/Puerto/${id}`));
};

export const getPuertoAccounts = (text) => {
  return get(getApiUrlData(`/api/AccountData/Puerto/${text}`));
};

export const getPuertoByFilter = async (filter) => {
  const response = await fetch(
    getApiUrlData(`/api/AccountData/Puerto/${filter}`),
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
