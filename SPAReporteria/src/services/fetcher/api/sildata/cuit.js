import get from '../serviceApi'
import { getApiUrlData } from 'config'

export const getAllCuitAccounts = () => {
  return get(getApiUrlData("/api/AccountData/Cuit"))
}

export const getCuitAccount = (id) => {
  return get(getApiUrlData(`/api/AccountData/Cuit/${id}`))
}

export const getCuitAccounts = (text) => {
  return get(getApiUrlData(`/api/AccountData/Cuit/${text}`))
}