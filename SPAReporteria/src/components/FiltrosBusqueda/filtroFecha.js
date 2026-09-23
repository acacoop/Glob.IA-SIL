import { DatePicker } from '@mui/x-date-pickers';
import { TextField, Typography } from '@mui/material';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import { AdapterDayjs } from '@mui/x-date-pickers/AdapterDayjs';
import dayjs from 'dayjs';

const FiltroFecha = ({ fecha, setFecha }) => (
  <LocalizationProvider dateAdapter={AdapterDayjs}>
    <Typography variant="subtitle1" sx={{ mb: 1 }}>Fecha</Typography>
    <DatePicker
      value={dayjs(fecha)}
      onChange={(newValue) => setFecha(newValue)}
      format="DD/MM/YYYY"
      renderInput={(params) => (
        <TextField
          {...params}
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
        />
      )}
    />
  </LocalizationProvider>
);

export default FiltroFecha;
