import { useState, useEffect } from "react"
import { Select, MenuItem, InputLabel, FormControl } from "@mui/material"

import { useTheme } from "@emotion/react"

export default function InputSelect({ id, label, fetcher, optionsSetter, ...props }) {
  const [items, setItems] = useState([])
  const theme = useTheme()
  useEffect(() => {
    fetcher().then(result => setItems(optionsSetter(result)))
  }, [])
  return (
    <FormControl fullWidth sx={{ ...theme.typography.customInput }}>
      <InputLabel htmlFor={id}>{label}</InputLabel>
      <Select
          label={label}
          variant="outlined"
          {...props}
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