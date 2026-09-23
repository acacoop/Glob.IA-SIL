import get from '../serviceApi'
import { getApiUrlComunicaciones } from 'config'

export const getAllProducts = () => {
  return get(getApiUrlComunicaciones("/api/configuration/productos"))
}

export const getProduct = (id) => {
  return get(getApiUrlComunicaciones(`/api/configuration/productos/${id}`))
}

export const getProducts = (text) => {
  return get(getApiUrlComunicaciones(`/api/configuration/productos/?fieldname=nombre&value=${text}`))
}
