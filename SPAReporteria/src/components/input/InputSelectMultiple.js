import { useState, useEffect } from "react"
import {
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  ListItemText,
  CircularProgress,
} from "@mui/material";
import Checkbox from '@mui/material/Checkbox';
import CheckBoxOutlineBlankIcon from "@mui/icons-material/CheckBoxOutlineBlank";
import CheckBoxIcon from "@mui/icons-material/CheckBox";
import { useTheme } from "@emotion/react"

const icon = <CheckBoxOutlineBlankIcon fontSize="small" />;
const checkedIcon = <CheckBoxIcon fontSize="small" />;

export default function InputSelectMultiple({ id, label, fetcher, optionsSetter, value = [], ...props }) {
  const [items, setItems] = useState([])
  const theme = useTheme()
  useEffect(() => {
    fetcher().then(result => setItems(optionsSetter(result)))
  }, [])
  return (
    <FormControl fullWidth sx={{ ...theme.typography.customInput }}>
      <InputLabel htmlFor={id}>{label}</InputLabel>
      <Select
          label={label}
          variant="outlined"
          multiple
          value={value}
          {...props}
                renderOption={(props, option, { selected }) => (
                  <li {...props}>
                    <Checkbox
                      icon={icon}
                      checkedIcon={checkedIcon}
                      style={{ marginRight: 8 }}
                      checked={selected}
                    />
                    {option.label}
                  </li>
                )}
        >
        {/* {
          items.length > 0 ? 
            items.map(item => 
            <MenuItem value={item.value} key={item.value}>
                <Checkbox checked={value.includes(item.value)} 
                    icon={icon}
                    checkedIcon={checkedIcon}
                    style={{ marginRight: 8 }} />
                <ListItemText primary={item.label} />
            </MenuItem>) :
            <MenuItem value={""} key={"Vacío"}>{"Vacío"}</MenuItem>
        } */}
      </Select>
    </FormControl>
  )
}