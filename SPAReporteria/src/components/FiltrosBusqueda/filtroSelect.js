import { TextField, Typography, MenuItem } from '@mui/material';

const FiltroSelect = ({ label, value, setValue, opciones }) => (
  <>
    <Typography variant="subtitle1" sx={{ mb: 1 }}>{label}</Typography>
    <TextField
      select
      value={value}
      onChange={(e) => setValue(e.target.value)}
      size="small"
      fullWidth
      variant="standard"
      sx={{
        '& .MuiInputBase-root': {
          border: '1px solid #ccc',
          borderRadius: '4px',
          padding: '4px 8px'
        },
        '& .MuiInput-underline:before': {
          borderBottom: 'none'
        }
      }}
    >
      {opciones.map((opt, i) => (
        <MenuItem key={i} value={opt}>
          {opt}
        </MenuItem>
      ))}
    </TextField>
  </>
);

export default FiltroSelect;
