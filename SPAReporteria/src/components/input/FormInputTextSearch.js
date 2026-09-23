// react
import { useState } from 'react';

// mui
import { Box, InputAdornment, OutlinedInput } from '@mui/material';
import { useTheme, styled } from '@mui/material/styles';
import { shouldForwardProp } from '@mui/system';
import SearchIcon from '@mui/icons-material/Search';

const OutlineInputStyle = styled(OutlinedInput, { shouldForwardProp })(({ theme }) => ({
  minWidth: '50%',
  paddingLeft: 16,
  paddingRight: 16,
  '& input': {
      background: 'transparent !important',
      paddingLeft: '4px !important'
  },
  [theme.breakpoints.down('lg')]: {
    width: '100%'
  },
  [theme.breakpoints.down('md')]: {
      width: '100%',
      background: '#fff'
  }
}));

export default function FormInputTextSearch({value, onChange, others}) {
  const theme = useTheme();

  return (
    <Box>
        <OutlineInputStyle
            id="input-search-header"
            value={value}
            onChange={onChange}
            placeholder="Buscar"
            startAdornment={
                <InputAdornment position="start">
                    <SearchIcon stroke={1.5} size="1rem" color={theme.palette.grey[500]} />
                </InputAdornment>
            }
            aria-describedby="search-helper-text"
            inputProps={{ 'aria-label': 'weight' }}
            {...others}
        />
    </Box>
  )
}