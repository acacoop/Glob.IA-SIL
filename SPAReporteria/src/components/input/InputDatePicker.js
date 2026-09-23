import { DesktopDatePicker } from '@mui/x-date-pickers/DesktopDatePicker';
import { styled } from '@mui/material/styles';

import React from 'react'

const DatePickerStyled = styled(DesktopDatePicker)(({ theme }) => ({
  width: "100%",
  marginTop: 8,
  marginBottom: 8,
  '& label':{
    top: 23,
    left: 0,
    color: theme.grey500,
    '&[data-shrink="false"]': {
        top: 5
    }
  },
  '& legend': {
    display: "none"
  },
  '& fieldset': {
    top: 0
  },
}));

const InputDatePicker = React.forwardRef(({...props}, inputRef) => {
  return <DatePickerStyled {...props} />
})

export default InputDatePicker;