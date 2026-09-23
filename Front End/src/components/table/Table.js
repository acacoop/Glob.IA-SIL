import { Box, CircularProgress, Table, TableHead, TableBody, TableFooter, TablePagination, TableRow, TableCell } from '@mui/material';
import { styled } from '@mui/material/styles';

const HeaderStyledTableCell = styled(TableCell)(({ theme }) => ({
  fontWeight: 'bold',
  fontSize: '1rem'
}));

const ButtonStyledTableCell = styled(TableCell)(({ theme }) => ({
  width: '50px'
}));

const BodyStyledTableRow = styled(TableRow)(({ theme }) => ({
  '&:nth-of-type(odd)': {
    backgroundColor: theme.palette.grey[100],
  },
  // hide last border
  '&:last-child td, &:last-child th': {
    border: 0,
  },
}));

const WaitingResponse = () => {
  return <Box sx={{ display: 'flex', alignItems: 'center' }}>
    <CircularProgress />
  </Box>
}

export { HeaderStyledTableCell, BodyStyledTableRow, ButtonStyledTableCell, WaitingResponse }