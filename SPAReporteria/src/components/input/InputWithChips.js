import PropTypes from "prop-types";
import { Chip, TextField, Autocomplete } from "@mui/material";

const InputWithChips = ({ id, placeholder, options = [], onChange, name, inputRef, ...props }) => {
  return <Autocomplete
        multiple
        id={id}
        options={options}
        freeSolo
        onChange={onChange}
        renderTags={(value, getTagProps) =>
          value.map((option, index) => (
            <Chip
              variant="outlined"
              label={option}
              {...getTagProps({ index })}
            />
          ))
        }
        renderInput={(params) => (
          <TextField
            {...params}
            name={name}
            variant="outlined"
            placeholder={placeholder}
            inputRef={inputRef}
          />
        )}
      />
}

InputWithChips.defaultProps = {
  options: []
};

InputWithChips.propTypes = {
  onChange: PropTypes.func.isRequired,
  placeholder: PropTypes.string,
  id: PropTypes.string,
  options: PropTypes.arrayOf(PropTypes.string)
}

export default InputWithChips;