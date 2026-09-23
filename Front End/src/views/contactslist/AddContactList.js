import { useState } from "react"
import FormContactList from "./FormContactList";
import MainCard from "components/cards/MainCard";
import { addContactList } from "services/contactlist";
import { AlertResponse } from "components/alert"

export default function AddListContacts() {
  const [apiResponse, setApiResponse] = useState({})

  const handleSubmit = (values) => {
    addContactList({
      nombre: values.nombre,
      descripcion: values.descripcion,
      listasHijas: values.contactos?.filter(contacto => contacto.obj.esLista).map(contacto => contacto.obj.id),
      personas: values.contactos?.filter(contacto => !contacto.obj.esLista).map(contacto => contacto.obj.id)
    }).then(response => {
      setApiResponse(response)
    })
  }

  return (
    <MainCard title={"Nueva Lista de Contactos"}>
      <FormContactList onSubmit={handleSubmit}/>
      <AlertResponse response={apiResponse} successMessage={"Agregado correctamente"} />
    </MainCard>
  )
}