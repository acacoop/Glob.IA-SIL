import { Grid, Skeleton } from '@mui/material';
import InputSkeleton from 'components/input/skeleton/InputSkeleton';

export default function SkeletonFormContact({title}) {
  return (
    <Grid container spacing={2}>
      <Grid item xs={6}>
        <InputSkeleton />
      </Grid>
      <Grid item xs={6}>
        <InputSkeleton />
      </Grid>
      <Grid item xs={6}>
        <InputSkeleton />
      </Grid>
      <Grid item xs={6}>
        <InputSkeleton />
      </Grid>
      <Grid item xs={6}>
        <InputSkeleton />
      </Grid>
      <Grid item xs={6}>
        <InputSkeleton />
      </Grid>
      <Grid container direction="row" spacing={2} justifyContent="flex-end">
        <Grid item>
          <Skeleton variant="rectangular" />
        </Grid>
        <Grid item>
          <Skeleton variant="rectangular" />
        </Grid>
      </Grid>
    </Grid>
  )
}