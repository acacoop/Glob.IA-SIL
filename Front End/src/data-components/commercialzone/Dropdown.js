import React from 'react'
import FormSelect from "components/input/FormSelect"
import useApi from 'hooks/useApi'
import { getAllZones } from 'services/commercialzone'
import InputSkeleton from "components/input/skeleton/InputSkeleton"

const DroopdownComercialZone = React.forwardRef(({value, onBlur, onChange, touched, errors}, ref) => {
  const { isLoading, data } = useApi(getAllZones)
  const emptyOption = { value:"", label: "Ninguna" }
  
  return isLoading ? (
    <InputSkeleton />
  ) : (
    <FormSelect
      value={value || ''}
      onBlur={onBlur}
      onChange={onChange}
      items={[emptyOption, ...data?.map(zona => { return { value: zona.id, label: zona.nombre } })]}
      label={"Zona Comercial"}
      name="zona"
      onError={value => {
        return Boolean(touched?.zona && errors?.zona)
      }}
      error={errors?.zona}
      inputRef={ref}
    />
  )
})

export default DroopdownComercialZone;