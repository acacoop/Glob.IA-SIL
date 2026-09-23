import get from '../serviceApi'
import { getApiUrlComunicaciones } from 'config'

export const getAllCenters = () => {
  return get(getApiUrlComunicaciones("/api/configuration/centro"))
}

export const getCenter = (id) => {
  return get(getApiUrlComunicaciones(`/api/configuration/centro/${id}`))
}

export const getCenters = (text) => {
  return get(getApiUrlComunicaciones(`/api/configuration/centro/?q=${text}`))
}