import { Grid, Skeleton } from '@mui/material'
import InputSkeleton from 'components/input/skeleton/InputSkeleton';
import MainCard from "components/cards/MainCard";
import FormButtonLink from 'components/button/Link'

export default function EditContactListSkeleton() {
  return (
    <Grid container spacing={2}>
      <Grid item xs={9}>
        <MainCard title={"Editar Lista de Contactos"}>
          <Grid container spacing={2}>
            <Grid item md={6} sm={12} xs={12}>
              <InputSkeleton /> 
            </Grid>
            <Grid item md={6} sm={12} xs={12}>
              <InputSkeleton />
            </Grid>
            <Grid item xs={12}>
              <InputSkeleton /> 
            </Grid>
            <Grid item xs={12}>
              <Grid container direction="row" spacing={2} justifyContent="flex-end">
                <Grid item>
                  <FormButtonLink
                    to="/listas-contactos"
                    variant="cancel"
                  >
                    Cancelar
                  </FormButtonLink>
                </Grid>
                <Grid item>
                  <Skeleton variant="rectangular" height={44} width={100}/>
                </Grid>
              </Grid>
            </Grid>
          </Grid>
        </MainCard>
      </Grid>
      <Grid item xs={3}>
        <MainCard>
          <Skeleton variant='rectangular' height={15} />
        </MainCard>
      </Grid>
    </Grid>
  )
}