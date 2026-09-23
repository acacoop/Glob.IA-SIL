import get from '../serviceApi'
import { getApiUrl } from 'config'

export const getAllRoles = () => {
  return get(getApiUrl("/api/configuration/rol"))
}

export const getRol = (id) => {
  return get(getApiUrl(`/api/configuration/rol/${id}`))
}