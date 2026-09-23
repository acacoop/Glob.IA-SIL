import get from '../serviceApi'
import { getApiUrl } from 'config'

export const getAllCenters = () => {
  return get(getApiUrl("/api/configuration/centro"))
}

export const getCenter = (id) => {
  return get(getApiUrl(`/api/configuration/centro/${id}`))
}