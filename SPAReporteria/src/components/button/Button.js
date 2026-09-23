// mui
import { Avatar, ButtonBase, Button } from '@mui/material';
import { useTheme } from '@mui/material/styles';
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import AnimateButton from 'components/extended/AnimateButton'
import { LoadingButton } from '@mui/lab';

const getColor = (color, variant, theme) => {
  if (color) return color
  if (variant === 'cancel') return cancelVariant(theme)
  return defaultColor(theme)
}

const defaultColor = theme => { 
  return {
    background: "white",
    border: `1px solid ${theme.palette.primary.dark}`,
    color: theme.palette.primary.dark,
    '&:hover': {
        background: theme.palette.primary.dark,
        color: "white"
    }
  }
}

const cancelVariant = theme => {
  return {
    background: "white",
    border: `1px solid ${theme.palette.error.dark}`,
    color: theme.palette.error.dark,
    '&:hover': {
        background: theme.palette.error.dark,
        color: "white"
    }
  }
}

export default function FormButton({onClick, isSubmitting=false, color, variant, size ="large", others, children}) {
  const theme = useTheme();
  return (
    <AnimateButton>
      <Button
          disableElevation
          disabled={isSubmitting}
          size={size}
          type="submit"
          variant="contained"
          sx={getColor(color, variant, theme)}
          onClick={onClick}
          {...others}
      >
          {children}
      </Button>
  </AnimateButton>
  )
}

export function FormLoadingButton({onClick, isLoading, isSubmitting=false, color, variant, size ="large", others, children}) {
  const theme = useTheme();
  return (
    <AnimateButton>
      <LoadingButton
          disableElevation
          disabled={isSubmitting}
          loading={isLoading}
          size={size}
          type="submit"
          variant="contained"
          sx={getColor(color, variant, theme)}
          onClick={onClick}
          {...others}
      >
          {children}
      </LoadingButton>
  </AnimateButton>
  )
}

export function FormIconButton({onClick, color, disabled, children}) {
  const theme = useTheme();
  return <ButtonBase 
      disabled={disabled}
      sx={{ borderRadius: '12px', overflow: 'hidden', border: `1px solid ${color?.border || theme.palette.primary.dark}` }}
    >
    <Avatar
        variant="rounded"
        sx={{
            ...theme.typography.commonAvatar,
            ...theme.typography.mediumAvatar,
            transition: 'all .2s ease-in-out',
            background: color?.background || "white",
            color: color?.color || theme.palette.primary.dark,
            '&:hover': {
                background: color?.hover?.background || theme.palette.primary.dark,
                color: color?.hover?.color || theme.palette.primary.light
            }
        }}
        onClick={onClick}
    >
        { children }
    </Avatar>
  </ButtonBase>
}

export function ButtonEdit({onClick, color}) {
  const theme = useTheme();
  return <FormIconButton onClick={onClick} color={
    {
      background: theme.palette.warning.light, 
      color: theme.palette.warning.dark,
      hover: {
        background: theme.palette.warning.dark,
        color: theme.palette.warning.light
      }
    }
  }>
    <EditIcon />
  </FormIconButton>
}

export function ButtonDelete({onClick, color}) {
  const theme = useTheme();
  return <FormIconButton onClick={onClick} color={
    {
      background: theme.palette.error.light, 
      color: theme.palette.error.dark,
      hover: {
        background: theme.palette.error.dark,
        color: theme.palette.error.light
      }
    }
  }>
    <DeleteIcon />
  </FormIconButton>
}