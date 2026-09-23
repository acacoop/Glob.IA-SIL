import get, { add, edit, remove } from '../serviceApi'
import { getApiUrl } from 'config'

export const getAllPersons = () => {
  return get(getApiUrl("/api/contacto/personas"))
}

export const getPersons = (q, p) => {
  return get(getApiUrl(`/api/contacto/personas/?q=${q}&p=${p}`))
}

export const getPerson = (id) => {
  return get(getApiUrl(`/api/contacto/persona/${id}`))
}

export const addPerson = (data) => {
  return add(getApiUrl("/api/contacto/persona"), data)
}

export const editPerson = (data, id) => {
  return edit(getApiUrl(`/api/contacto/persona/${id}`), data)
}

export const removePerson = (id) => {
  return remove(getApiUrl("/api/contacto/persona"), id)
}