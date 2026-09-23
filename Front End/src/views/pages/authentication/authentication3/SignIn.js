import { useEffect } from 'react'
import AuthService from "services/auth/AuthService"
import AuthWrapper1 from '../AuthWrapper1';
import { useNavigate } from 'react-router-dom';
import { Jelly } from '@uiball/loaders'
import { useTheme } from '@mui/material/styles';
import { Typography, useMediaQuery } from '@mui/material';
import useAuth from 'hooks/useAuth';

export default function SignIn() {
  const theme = useTheme();
  const navigate = useNavigate();
  const matchDownSM = useMediaQuery(theme.breakpoints.down('md'));
  const { onLogin } = useAuth();

  useEffect(() => {
      AuthService.getInstance().get().signinCallback().then(user => {
        onLogin(user)
        navigate("/")
      })
      .catch(err => console.error(err))
  });
  
  return (
    <AuthWrapper1 sx={{display: "flex", justifyContent: "center", alignItems: "center", flexDirection: "column"}}>
      <Typography
          color={theme.palette.secondary.main}
          gutterBottom
          variant={matchDownSM ? 'h3' : 'h2'}
          sx={{marginBottom: "2rem"}}
      >
        Iniciando sesión
      </Typography>
      <Jelly size={80} color={theme.palette.secondary.dark} />
    </AuthWrapper1>
  );
}