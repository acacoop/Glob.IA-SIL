import React from 'react'
import FormSelect from "components/input/FormSelect"
import useApi from 'hooks/useApi'
import { getAllCenters } from 'services/center'
import InputSkeleton from "components/input/skeleton/InputSkeleton"

const DroopdownCenters = React.forwardRef(({value, onBlur, onChange, touched, errors}, ref) => {
  const { isLoading, data } = useApi(getAllCenters)
  const emptyOption = { value:"", label: "Ninguno" }
  
  return isLoading ? (
    <InputSkeleton />
  ) : (
    <FormSelect
      value={value || ''}
      onBlur={onBlur}
      onChange={onChange}
      items={[emptyOption, ...data?.map(centro => { return { value: centro.id, label: centro.nombre } })]}
      label={"Centro"}
      name="centro"
      onError={value => {
        return Boolean(touched?.centro && errors?.centro)
      }}
      error={errors?.centro}
      inputRef={ref}
    />
  )
})

export default DroopdownCenters;