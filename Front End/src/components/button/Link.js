// mui
import { Link } from 'react-router-dom';
import { useTheme } from '@mui/material/styles';
import FormButton from './Button'

import AnimateButton from 'components/extended/AnimateButton'

export default function FormButtonLink({onClick, color, variant, others, to, children}) {
  const theme = useTheme();
  
  return (
    <FormButton onClick={onClick} color={color} variant={variant} others={{component: Link, to: to}} {...others}>
      {children}
    </FormButton>
  )
}