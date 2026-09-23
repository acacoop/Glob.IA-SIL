import { FormControl, InputLabel, Select, FormHelperText, MenuItem } from "@mui/material"
import { useTheme } from '@mui/material/styles';

export default function FormSelect({ id, name, value, label, onChange, onBlur, onError, error, items, inputRef }) {
  const theme = useTheme()

  return (
    <FormControl fullWidth error={onError ? onError(value) : false} sx={{ ...theme.typography.customInput }}>
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
          inputRef={inputRef}
      >
        {
          items.length > 0 ? 
            items.map(item => <MenuItem value={item.value} key={item.value}>{item.label}</MenuItem>) :
            <MenuItem value={""} key={"Vacío"}>{"Vacío"}</MenuItem>
        }
      </Select>
      {onError && onError(value) && (
          <FormHelperText error id={`error-${id}`}>
              {error}
          </FormHelperText>
      )}
  </FormControl>
  )
}