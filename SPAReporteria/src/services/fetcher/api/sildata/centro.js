import get from "../serviceApi";
import { getApiUrlData, getApiSilData } from "config";

export const getAllCentroAccounts = () => {
  return get(getApiUrlData("/api/AccountData/Centro"));
};

export const getCentroAccount = (id) => {
  return get(getApiUrlData(`/api/AccountData/Centro/${id}`));
};

export const getCentroAccounts = (text) => {
  return get(getApiUrlData(`/api/AccountData/Centro/${text}`));
};

export const getCentroByFilter = async (filter) => {
  const response = await fetch(
    getApiUrlData(`/api/AccountData/Centro/${filter}`),
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
