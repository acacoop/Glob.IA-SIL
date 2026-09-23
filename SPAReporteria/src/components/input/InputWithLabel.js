import { InputLabel, FormControl } from "@mui/material"

import { useTheme } from "@emotion/react"

export default function InputWithLabel({ forName, label, children }) {
  const theme = useTheme()

  return (
    <FormControl fullWidth sx={{ ...theme.typography.customInput }}>
      <InputLabel htmlFor={forName}>{label}</InputLabel>
      { children }
    </FormControl>
  )
}