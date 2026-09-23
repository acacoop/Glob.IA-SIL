import { useState } from "react"
import MainCard from "components/cards/MainCard"
import FormContact from './FormContact'
import useApi from 'hooks/useApi'
import { getPerson } from "services/person"
import Skeleton from './skeleton/SkeletonFormContact'
import { editPerson } from "services/person"
import { AlertResponse } from "components/alert"

import { useParams } from 'react-router-dom'

export default function EditContact() {
  const params = useParams()
  const { isLoading, data } = useApi(getPerson, params.id)

  const [apiResponse, setApiResponse] = useState({})

  const handleSubmit = (values) => {
    editPerson(
      {
        id: params.id,
        apellido: values.apellido,
        nombre: values.nombre,
        cuenta: null,
        rol: values.rol,
        usuario: null,
        centro: values.centro,
        zonaComercial: values.zona,
        contactos: values.contactos?.map(contacto => {
          return {
            tipo: contacto.tipo.obj.id,
            dato: contacto.dato
          }
        })
      }, 
      params.id
    ).then(response => {
      setApiResponse(response)
    })
  }
  
  return (
    <MainCard title={"Editar Contacto"}>
      {
        isLoading ?
          <Skeleton title={"Editar Contacto"} /> :
          <FormContact initValues={
            {
              nombre: data.nombre, 
              apellido: data.apellido, 
              email: data.email, 
              rol: data.rol?.id, 
              zona: data.zonaComercial?.id, 
              centro: data.centro?.id,
              contactos: data.contactos.map(contacto => {
                return {
                  id: contacto.id,
                  dato: contacto.dato,
                  tipo: {
                    id: JSON.stringify(contacto.tipo),
                    obj: contacto.tipo
                  }
                }
              })
            }
          } 
          onSubmit={handleSubmit}/>
      }
      <AlertResponse response={apiResponse} successMessage={"Modificacdo correctamente"}/>
    </MainCard>
  )
}