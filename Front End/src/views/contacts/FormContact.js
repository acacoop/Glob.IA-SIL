import { useCallback } from 'react'
import { useForm, useFieldArray, Controller } from 'react-hook-form'
import { useYupValidationResolver } from 'hooks/useYupValidationResolver'

import DropdownRoles from 'data-components/rol/Dropdown'
import DropdownZonas from 'data-components/commercialzone/Dropdown'
import DropdownCentros from 'data-components/center/Dropdown'

import FormInputText from "components/input/FormInputText"
import { FormButton, FormButtonLink } from "components/button"
import AccordionContacts from 'data-components/contact/AccordionContacts'

import { Grid } from '@mui/material'

// third party
import * as Yup from 'yup';

const validations = Yup.object().shape({
  nombre: Yup.string().required('Requerido'),
  apellido: Yup.string().required('Requerido'),
});

export default function FormContact({initValues = {nombre: "", apellido: "", rol: "", zona: "", centro: "", contactos: []}, onSubmit}) {
  const resolver = useYupValidationResolver(validations);
  const { handleSubmit, register, control, formState: { errors } } = useForm({ resolver, defaultValues: initValues });
  const { replace } = useFieldArray({control, name: "contactos"})
  

  const memorizedOnChangeContacts = useCallback(contactos => {
    replace(contactos)
  }, [replace])

  const inputNombre = register("nombre")
  const inputApellido = register("apellido")
  
  return (
    <form onSubmit={handleSubmit(data => onSubmit(data))}>
      <Grid container spacing={2}>
        <Grid item xs={6}>  
          <FormInputText 
            label={"Nombre"} 
            name={inputNombre.name}
            onChange={inputNombre.onChange}
            onBlur={inputNombre.onBlur}
            inputRef={inputNombre.ref}
            onError={value => {
              return Boolean(errors?.nombre)
            }}
            error={errors?.nombre?.message}
          />
        </Grid>
        <Grid item xs={6}>
          <FormInputText 
            label={"Apellido"} 
            name={inputApellido.name}
            onChange={inputApellido.onChange}
            onBlur={inputApellido.onBlur}
            inputRef={inputApellido.ref}
            onError={value => {
              return Boolean(errors?.apellido)
            }}
            error={errors?.apellido?.message}
          />
        </Grid>
        <Grid item xs={6}>
          <Controller
            name="rol"
            control={control}
            render={({ field }) => <DropdownRoles {...field} />}
          />
        </Grid>
        <Grid item xs={6}>
          <Controller
            name="zona"
            control={control}
            render={({ field }) => <DropdownZonas {...field} />}
          />
        </Grid>
        <Grid item xs={6}>
          <Controller
            name="centro"
            control={control}
            render={({ field }) => <DropdownCentros {...field} />}
          />
        </Grid>
        <Grid item xs={12}>
          <AccordionContacts onChange={memorizedOnChangeContacts} values={initValues.contactos} />
        </Grid>
        <Grid container direction="row" spacing={2} justifyContent="flex-end">
          <Grid item>
            <FormButtonLink
              to="/contactos"
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
    </form>
  )
}