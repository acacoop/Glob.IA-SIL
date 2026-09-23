import { FormControl, InputLabel, FormHelperText } from "@mui/material"
import { useTheme } from '@mui/material/styles';

export function InputWithLabelAndValidation({ id, forName, label, errors, children }) {
  return (
    <InputWithValidation id={id} forName={forName} errors={errors}>
      <InputLabel htmlFor={id}>{label}</InputLabel>
      { children }
    </InputWithValidation>
  )
}

export function InputWithValidation({ id, forName, errors, children }) {
  const theme = useTheme()

  const handleError = () => {
    return Boolean(errors?.[forName])
  }

  return (
    <FormControl fullWidth error={handleError()} sx={{ ...theme.typography.customInput }}>
      { children }
      {handleError() && (
        <FormHelperText error id={`error-${id}`}>
          {errors?.[forName]?.message}
        </FormHelperText>
      )}
    </FormControl>
  )
}

export function InputWithOnlyValidation({ id, forName, errors, children }) {
  const handleError = () => {
    return Boolean(errors?.[forName])
  }

  return (
    <>
      { children }
      {handleError() && (
        <FormHelperText error id={`error-${id}`}>
          {errors?.[forName]?.message}
        </FormHelperText>
      )}
    </>
  )
}