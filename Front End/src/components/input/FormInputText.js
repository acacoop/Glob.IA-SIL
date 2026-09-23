import { FormControl, InputLabel, OutlinedInput, FormHelperText } from "@mui/material"
import { useTheme } from '@mui/material/styles';

export default function FormInputText({ id, name, value, label, type, onChange, onBlur, onError, inputRef, error }) {
  const theme = useTheme()

  return (
    <FormControl fullWidth error={onError ? onError(value) : false} sx={{ ...theme.typography.customInput }}>
      <InputLabel htmlFor={id}>{label}</InputLabel>
      <OutlinedInput
          id={id}
          type={type}
          value={value}
          name={name}
          onBlur={onBlur}
          onChange={onChange}
          label={label}
          inputProps={{}}
          inputRef={inputRef}
      />
      {onError && onError(value) && (
        <FormHelperText error id={`error-${id}`}>
          {error}
        </FormHelperText>
      )}
  </FormControl>
  )
}