import Checkbox from '@mui/material/Checkbox';
import Autocomplete from '@mui/material/Autocomplete';
import CheckBoxOutlineBlankIcon from '@mui/icons-material/CheckBoxOutlineBlank';
import CheckBoxIcon from '@mui/icons-material/CheckBox';
import TextField from '@mui/material/TextField';

const icon = <CheckBoxOutlineBlankIcon fontSize="small" />;
const checkedIcon = <CheckBoxIcon fontSize="small" />;

export default function FormAutocomplete({id, name, defaultValue, label, placeholder, onChange, onInputChange, options = []}) {

  return (
    <Autocomplete
      multiple
      defaultValue={defaultValue}
      options={options}
      disableCloseOnSelect
      getOptionLabel={(option) => option.title}
      onChange={onChange}
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
            onChange={onInputChange}
            placeholder={placeholder}
        />
      )}
    />
  );
};