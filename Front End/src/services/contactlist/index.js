import get, { add, edit, remove } from '../serviceApi'
import { getApiUrl } from 'config'

export const getAllContactsLists = () => {
  return get(getApiUrl("/api/contacto/listas"))
}

export const getContactsLists = (q, p) => {
  return get(getApiUrl(`/api/contacto/listas/?q=${q}&p=${p}`))
}

export const getContactList = (id) => {
  return get(getApiUrl(`/api/contacto/lista/${id}`))
}

export const addContactList = (lista) => {
  if (lista === undefined) throw new Error("Debe tener un objeto lista")
  return add(getApiUrl("/api/contacto/lista"), lista)
}

export const editContactList = (lista, id) => {
  if (lista === undefined) throw new Error("Debe tener un objeto lista")
  return edit(getApiUrl(`/api/contacto/lista/${id}`), lista)
}

export const removeContactList = (id) => {
  if (id === undefined) throw new Error("Debe tener una lista")
  return remove(getApiUrl("/api/contacto/listas"), id)
}