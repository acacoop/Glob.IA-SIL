import React, { useState } from 'react';
import { Autocomplete, TextField, CircularProgress, Typography } from '@mui/material';
import axios from 'axios';

const FiltroBusqueda = ({ label, endpoint, value, onChange }) => {
  const [opciones, setOpciones] = useState([]);
  const [loading, setLoading] = useState(false);
  const [open, setOpen] = useState(false);

  const handleInputChange = async (event, inputValue) => {
    if (inputValue.length < 3) {
      setOpciones([]);
      return;
    }

    setLoading(true);
    try {
      const response = await axios.get(`${endpoint}?query=${inputValue}`);
      // Asegurate que cada item tenga `.label` o ajustá el getOptionLabel abajo
      const mappedOptions = response.data.map(item => ({
        label: item.nombre || item.label || item, // lo que se muestra
        value: item.id || item.value || item      // lo que se guarda
      }));
      setOpciones(mappedOptions);
    } catch (error) {
      console.error('Error al buscar:', error);
      setOpciones([]);
    } finally {
      setLoading(false);
    }
  };

  return (
    <>
      <Typography variant="subtitle1" sx={{ mb: 1 }}>{label}</Typography>
      <Autocomplete
        open={open}
        onOpen={() => setOpen(true)}
        onClose={() => setOpen(false)}
        options={opciones}
        getOptionLabel={(option) => option.label || ''}
        inputValue={value}
        onInputChange={(event, newInputValue) => {
          onChange(newInputValue);
          handleInputChange(event, newInputValue);
        }}
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
            InputProps={{
              ...params.InputProps,
              endAdornment: (
                <>
                  {loading && <CircularProgress color="inherit" size={16} />}
                  {params.InputProps.endAdornment}
                </>
              )
            }}
          />
        )}
      />
    </>
  );
};

export default FiltroBusqueda;
