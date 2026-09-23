import get from '../serviceApi'
import { getApiUrl } from 'config'

export const getAllTypes = () => {
  return get(getApiUrl("/api/configuration/tipocontacto"))
}

export const getType = (id) => {
  return get(getApiUrl(`/api/configuration/tipocontacto/${id}`))
}