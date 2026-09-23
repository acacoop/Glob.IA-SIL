import { useState, useEffect, forwardRef } from 'react';

import Snackbar from '@mui/material/Snackbar';
import MuiAlert from '@mui/material/Alert';
import AlertResponse from './AlertResponse';

const Alert = forwardRef(function Alert(props, ref) {
  return <MuiAlert elevation={6} ref={ref} variant="filled" {...props} />;
});

const BasicAlert = ({message, severity, showAlert = false}) => {
  const [open, setOpen] = useState(false);

  useEffect(() => {
    setOpen(showAlert)
  }, [showAlert])

  const handleClose = (event, reason) => {
    if (reason === 'clickaway') {
      return;
    }

    setOpen(false);
  };

  return (
    <Snackbar open={open} autoHideDuration={6000} onClose={handleClose}>
      <Alert onClose={handleClose} severity={severity} sx={{ width: '100%' }}>
        {message}
      </Alert>
    </Snackbar>
  )
}

export const SuccessAlert = ({message, showAlert = false}) => {
  return <BasicAlert message={message} showAlert={showAlert} severity="success" />
}

export const ErrorAlert = ({message, showAlert = false}) => {
  return <BasicAlert message={message} showAlert={showAlert} severity="error" />
}

export { AlertResponse };