import React from 'react'
import FormSelect from "components/input/FormSelect"
import useApi from 'hooks/useApi'
import { getAllRoles } from 'services/rol'
import InputSkeleton from "components/input/skeleton/InputSkeleton"

const DroopdownRoles = React.forwardRef(({value, onChange, onBlur, name, touched, errors}, ref) => {
  const { isLoading, data } = useApi(getAllRoles)
  const emptyOption = { value:"", label: "Ninguno" }
  
  return isLoading ? (
    <InputSkeleton />
  ) : (
    <FormSelect
      value={value || ''}
      onBlur={onBlur}
      onChange={onChange}
      items={[emptyOption, ...data?.map(rol => { return { value: rol.id, label: rol.nombre } })]}
      label={"Rol"}
      name="rol"
      onError={value => {
        return Boolean(touched?.rol && errors?.rol)
      }}
      error={errors?.rol}
      inputRef={ref}
    />
  )
})

export default DroopdownRoles;