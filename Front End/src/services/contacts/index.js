import get from '../serviceApi'
import { getApiUrl } from 'config'

export const getAllContacts = () => {
  return get(getApiUrl("/api/contacto/contactos"))
}

export const getContacts = (q, p) => {
  return get(getApiUrl(`/api/contacto/contactos/?q=${q}&p=${p}`))
}

export const getContact = (id) => {
  return get(getApiUrl(`/api/contacto/contactos/${id}`))
}

export const addContact = () => {

}