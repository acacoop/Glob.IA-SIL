import { useState } from "react"
import MainCard from "components/cards/MainCard"
import FormContact from './FormContact'
import { addPerson } from "services/person"
import { AlertResponse } from "components/alert"

export default function AddContact() {
  const [apiResponse, setApiResponse] = useState({})

  const handleSubmit = (values) => {
    addPerson(
      {
        apellido: values.apellido,
        nombre: values.nombre,
        cuenta: null,
        rol: values.rol?.id,
        usuario: null,
        centro: values.centro?.id,
        zonaComercial: values.zonaComercial?.id,
        contactos: values.contactos?.map(contacto => {
          return {
            tipo: contacto.tipo.obj.id,
            dato: contacto.dato
          }
        })
      }
    ).then(response => {
      setApiResponse(response)
    })
  }

  return (
    <MainCard title={"Nuevo Contacto"}>
      <FormContact onSubmit={handleSubmit}/>
      <AlertResponse response={apiResponse} successMessage={"Agregado correctamente"}/>
    </MainCard>
  )
}