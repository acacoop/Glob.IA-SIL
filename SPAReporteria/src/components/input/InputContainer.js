import { useState, useEffect } from "react"
import { Select, MenuItem, InputLabel, FormControl } from "@mui/material"

import { useTheme } from "@emotion/react"

export default function InputSelect({ id, name, value, label, onChange, onBlur, fetcher, optionsSetter, ...props }) {
  const [items, setItems] = useState([])
  const theme = useTheme()

  useEffect(() => {
    fetcher().then(result => setItems(optionsSetter(result)))
  }, [fetcher, optionsSetter])

  return (
    <FormControl fullWidth sx={{ ...theme.typography.customInput }}>
      <InputLabel htmlFor={id}>{label}</InputLabel>
      <Select
          id={id}
          value={value}
          name={name}
          onBlur={onBlur}
          onChange={onChange}
          label={label}
          inputProps={{}}
          variant="outlined"
      >
        {
          items.length > 0 ? 
            items.map(item => <MenuItem value={item.value} key={item.value}>{item.label}</MenuItem>) :
            <MenuItem value={""} key={"Vacío"}>{"Vacío"}</MenuItem>
        }
      </Select>
    </FormControl>
  )
}