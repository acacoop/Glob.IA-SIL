import get from '../serviceApi'
import { getApiUrl } from 'config'

export const getAllZones = () => {
  return get(getApiUrl("/api/configuration/zonacomercial"))
}

export const getZone = (id) => {
  return get(getApiUrl(`/api/configuration/zonacomercial/${id}`))
}