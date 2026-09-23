import { useState, useEffect, useRef } from "react";
import Checkbox from "@mui/material/Checkbox";
import Autocomplete from "@mui/material/Autocomplete";
import CheckBoxOutlineBlankIcon from "@mui/icons-material/CheckBoxOutlineBlank";
import CheckBoxIcon from "@mui/icons-material/CheckBox";
import TextField from "@mui/material/TextField";
import PropTypes from "prop-types";

const icon = <CheckBoxOutlineBlankIcon fontSize="small" />;
const checkedIcon = <CheckBoxIcon fontSize="small" />;

const InputAutocomplete = ({
  id,
  name,
  value,
  label,
  placeholder,
  onChange,
  onBlur,
  inputRef,
  fetcher,
  optionsSetter,
}) => {
  const [inputValue, setInputValue] = useState("");
  const [options, setOptions] = useState([]);
  const [open, setOpen] = useState(false);
  const timerRef = useRef(null);

  // Limpio timeout al desmontar
  useEffect(() => {
    return () => {
      if (timerRef.current) clearTimeout(timerRef.current);
    };
  }, []);

  // Debounce + limpieza cuando inputValue está vacío
  useEffect(() => {
    if (timerRef.current) clearTimeout(timerRef.current);

    const q = inputValue.trim();

    if (q.length <= 1) {
      setOptions([]); // ✅ SIN resultados si no hay texto
      setOpen(false); // ✅ cerrar popup
      return;
    }

    timerRef.current = setTimeout(async () => {
      const results = await fetcher(q);
      setOptions(optionsSetter(results) || []);
      setOpen(true); // abrir cuando hay resultados
    }, 300);
  }, [inputValue, fetcher, optionsSetter]);

  return (
    <Autocomplete
      multiple
      defaultValue={value}
      options={options}
      open={open}
      onOpen={() => setOpen(inputValue.trim().length > 1 && options.length > 0)}
      onClose={() => setOpen(false)}
      inputValue={inputValue}
      onInputChange={(event, newValue, reason) => {
        if (reason === "clear") {
          // click en la X de limpiar
          setInputValue("");
          setOptions([]);
          setOpen(false);
          return;
        }
        setInputValue(newValue);
      }}
      disableCloseOnSelect
      getOptionLabel={(option) => option.title}
      filterOptions={(x) => x} // no re-filtrar en cliente
      onChange={onChange}
      onBlur={(e) => {
        onBlur?.(e);
        // si queda vacío al salir, limpiar por las dudas
        if (inputValue.trim() === "") {
          setOptions([]);
          setOpen(false);
        }
      }}
      noOptionsText={
        inputValue.trim().length <= 1
          ? "Escribí al menos 2 caracteres"
          : "Sin resultados"
      }
      renderOption={(props, option, { selected }) => (
        <li {...props}>
          <Checkbox
            icon={icon}
            checkedIcon={checkedIcon}
            style={{ marginRight: 8 }}
            checked={selected}
          />
          {option.title}
        </li>
      )}
      renderInput={(params) => (
        <TextField
          {...params}
          id={id}
          name={name}
          label={label}
          placeholder={placeholder}
          inputRef={inputRef}
          InputLabelProps={{
            shrink: true, // fuerza que el label quede arriba
            style: {
              color: "#000", // color del label
              fontWeight: 600,
            },
          }}
          InputProps={{
            ...params.InputProps,
            style: {
              color: "#000",
            },
          }}                        
          sx={{
            "& .MuiInputBase-input::placeholder": {
              color: "#000 !important", //color visible del placeholder
              opacity: 1,              //muestra el color completo
              fontWeight: 400,
            },
          }}
        />
      )}
    />
  );
};

InputAutocomplete.propTypes = {
  id: PropTypes.string,
  name: PropTypes.string,
  value: PropTypes.any,
  label: PropTypes.string,
  placeholder: PropTypes.string,
  onChange: PropTypes.func,
  onBlur: PropTypes.func,
  fetcher: PropTypes.func,
  optionsSetter: PropTypes.func,
  inputRef: PropTypes.any,
};

export default InputAutocomplete;
