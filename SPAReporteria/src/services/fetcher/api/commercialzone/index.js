import get from '../serviceApi'
import { getApiUrlComunicaciones } from 'config'

export const getAllZones = () => {
  return get(getApiUrlComunicaciones("/api/configuration/zonacomercial"))
}

export const getZone = (id) => {
  return get(getApiUrlComunicaciones(`/api/configuration/zonacomercial/${id}`))
}

export const getZones = (text) => {
  return get(getApiUrlComunicaciones(`/api/configuration/zonacomercial/?fieldname=nombre&value=${text}`))
}