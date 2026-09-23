import get from "../serviceApi";
import { getApiUrlData, getApiSilData } from "config";

export const getAllSellerAccounts = () => {
  return get(getApiUrlData("/api/AccountData/Vendedor"));
};

export const getSellerAccount = (id) => {
  return get(getApiUrlData(`/api/AccountData/Vendedor/${id}`));
};

export const getSellerAccounts = (text) => {
  return get(getApiUrlData(`/api/AccountData/Vendedor/${text}`));
};

export const getVendedorByFilter = async (filter) => {
  const response = await fetch(
    getApiUrlData(`/api/AccountData/Vendedor/${filter}`),
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
