import get from '../serviceApi'
import { getApiUrlComunicaciones } from 'config'

export const getAllAccounts = () => {
  return get("/api/account/")
}

export const getAccount = (id) => {
  return get(getApiUrlComunicaciones(`/api/account/${id}`))
}

export const getAccounts = (text) => {
  return get(getApiUrlComunicaciones(`/api/account/?q=${text}`))
}