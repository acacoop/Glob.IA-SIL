import { useState, useCallback } from 'react'

import FormInputText from "components/input/FormInputText"
import { FormButton, FormButtonLink } from "components/button"
import FormAutocomplete from "components/input/FormAutocomplete";
import { getContacts } from "services/contacts";
import { useYupValidationResolver } from 'hooks/useYupValidationResolver'

import { Grid } from '@mui/material'

// third party
import * as Yup from 'yup';
import { useForm, useFieldArray } from 'react-hook-form'

const validations = Yup.object().shape({
  nombre: Yup.string().required('Requerido'),
});

export default function FormContacList({initValues = {
    nombre: '',
    descripcion: '',
    contactos: []
  }, selectableData, onSubmit}) {
  const [contacts, setContacts] = useState([])
  const [selectedContacts, setSelectedContacts] = useState([])

  const resolver = useYupValidationResolver(validations);
  const { handleSubmit, register, control, formState: { errors } } = useForm({ resolver, defaultValues: initValues });
  const { replace } = useFieldArray({control, name: "contactos"})

  const inputNombre = register("nombre")
  const inputDescripcion = register("descripcion")
  let timer = null;

  const handleOnInputChange = e => {
    let text = e.currentTarget.value
    if (text.length > 3) {
      if (timer) {
        clearTimeout(timer)
        timer = null
      }

      timer = setTimeout(() => {
        getContacts(text, 1).then(results => {
          setContacts(
            results.map(result => { return { title: result.nombre, obj: result } })
          )
        })
      }, 300)
    }
  }

  const memorizedOnChangeContacts = useCallback((event, value) => {
    replace(value)
  }, [replace])

  return (
    <form onSubmit={handleSubmit(data => onSubmit(data))}>
      <Grid container spacing={2}>
        <Grid item md={6} sm={12} xs={12}>
          <FormInputText 
            label={"Nombre"} 
            name={inputNombre.name} 
            onBlur={inputNombre.onBlur}
            onChange={inputNombre.onChange}
            inputRef={inputNombre.ref}
            onError={value => {
              return Boolean(errors?.nombre)
            }}
            error={errors?.nombre?.message}
          />
        </Grid>
        <Grid item md={6} sm={12} xs={12}>
          <FormInputText 
            label={"Descripcion"} 
            name={inputDescripcion.name} 
            onBlur={inputDescripcion.onBlur}
            onChange={inputDescripcion.onChange}
            inputRef={inputDescripcion.ref}
          />
        </Grid>
        <Grid item xs={12}>
          <FormAutocomplete 
            defaultValue={initValues.contactos?.map(contacto => { return { title: contacto.nombre, obj: contacto } })}
            placeholder={"Contactos"}
            onInputChange={e => handleOnInputChange(e)}
            onChange={memorizedOnChangeContacts}
            options={contacts}
          />
        </Grid>
        <Grid item xs={12}>
          <Grid container direction="row" spacing={2} justifyContent="flex-end">
            <Grid item>
              <FormButtonLink
                to="/listas-contactos"
                variant="cancel"
              >
                Cancelar
              </FormButtonLink>
            </Grid>
            <Grid item>
              <FormButton>Aceptar</FormButton>
            </Grid>
          </Grid>
        </Grid>
      </Grid>
    </form>
  )
}