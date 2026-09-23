import FormSelect from "components/input/FormSelect"
import useApi from 'hooks/useApi'
import { getAllTypes } from 'services/typeofcontact'
import InputSkeleton from "components/input/skeleton/InputSkeleton"

export default function DroopdownTipos({value = "", onBlur, onChange}) {
  const { isLoading, data } = useApi(getAllTypes)
  const emptyOption = { value:"", label: "Ninguno" }
  
  return isLoading ? (
    <InputSkeleton />
  ) : (
    <FormSelect
      value={value}
      onBlur={onBlur}
      onChange={onChange}
      items={[emptyOption, ...data?.map(tipo => { return { value: JSON.stringify(tipo), label: tipo.nombre } })]}
      label={"Tipo Contacto"}
      name={"tipoContacto"}
    />
  )
}