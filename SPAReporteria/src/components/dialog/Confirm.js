import Dialog from '@mui/material/Dialog';
import DialogActions from '@mui/material/DialogActions';
import DialogContent from '@mui/material/DialogContent';
import DialogContentText from '@mui/material/DialogContentText';
import DialogTitle from '@mui/material/DialogTitle';
import { FormButton } from "components/button"
import ErrorOutlineIcon from '@mui/icons-material/ErrorOutline';
import { useTheme } from '@mui/material/styles'


export default function Confirm({open, title, message, handleClose, handleAgree}) {
  const theme = useTheme()
  return (
    <Dialog
        open={open}
        onClose={handleClose}
        aria-labelledby="alert-dialog-title"
        aria-describedby="alert-dialog-description"
      >
      <DialogTitle id="alert-dialog-title" sx={{textAlign: 'center', fontSize: 16}}>
        <ErrorOutlineIcon sx={{color: theme.palette.primary.dark, display: 'block', fontSize: 60, margin: 'auto', marginBottom: 1}}/>
        {title}
      </DialogTitle>
      <DialogContent>
        <DialogContentText id="alert-dialog-description">
          {message}
        </DialogContentText>
      </DialogContent>
      <DialogActions>
        <FormButton variant="cancel" onClick={handleClose}>Cerrar</FormButton>
        <FormButton onClick={handleAgree} autoFocus>
          Aceptar
        </FormButton>
      </DialogActions>
    </Dialog>
  )
}