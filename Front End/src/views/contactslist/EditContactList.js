import { useState } from "react"
import ContactListComposition from "./ContactListComposition"
import FormContactList from "./FormContactList";
import MainCard from "components/cards/MainCard";
import EditContactListSkeleton from './EditContactListSkeleton'
import useApi from 'hooks/useApi'
import { AlertResponse } from "components/alert"

import { getContactList, editContactList } from "services/contactlist";

import { Grid } from "@mui/material";

import { useParams } from 'react-router-dom'

export default function EditContactList() {
  const params = useParams()
  const { data, isLoading } = useApi(getContactList, params.id)
  const [apiResponse, setApiResponse] = useState({})

  const handleSubmit = (values) => {
    editContactList({
      id: params.id,
      nombre: values.nombre,
      descripcion: values.descripcion,
      listasHijas: values.contactos?.filter(contacto => contacto.obj.esLista).map(contacto => contacto.obj.id),
      personas: values.contactos?.filter(contacto => !contacto.obj.esLista).map(contacto => contacto.obj.id)
    },
    params.id).then(response => {
      setApiResponse(response)
    })
  }

  return (
    isLoading ?
      <EditContactListSkeleton /> :
      <Grid container spacing={2}>
        <Grid item xs={9}>
          <MainCard title={"Editar Lista de Contactos"}>
            <FormContactList initValues={data} onSubmit={handleSubmit}/>
          </MainCard>
        </Grid>
        <Grid item xs={3}>
          <MainCard>
            <ContactListComposition data={data} />
          </MainCard>
        </Grid>
        <AlertResponse response={apiResponse} successMessage={"Modificacdo correctamente"}/>
      </Grid>
  )
}