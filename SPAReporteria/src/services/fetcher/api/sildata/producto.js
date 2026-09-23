import get from "../serviceApi";
import { getApiUrlData, getApiSilData } from "config";

export const getAllProductos = () => {
  return get(getApiUrlData("/api/ProductData"));
};

export const getProducto = (id) => {
  return get(getApiUrlData(`/api/ProductData/${id}`));
};

export const getProductos = (text) => {
  return get(getApiUrlData(`/api/ProductData?filtro=${text}`));
};

export const getAllProducts_ = async () => {
  const response = await fetch(getApiUrlData(`/api/Datos/Productos`), {
    method: "GET",
    headers: {
      "Content-type": "application/json",
    },
  });
  const data = await response.json();
  return data;
};

export const getProductsByFilter = async (filter) => {
  const response = await fetch(
    getApiUrlData(`/api/Datos/ProductosByFilter/${filter}`),
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
