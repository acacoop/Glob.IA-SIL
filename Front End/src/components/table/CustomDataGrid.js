import { alpha, styled } from '@mui/material/styles';
import { DataGrid, gridClasses, esES, GridToolbar } from '@mui/x-data-grid';
import { LinearProgress } from '@mui/material'

const ODD_OPACITY = 0.2;

const StripedDataGrid = styled(DataGrid)(({ theme }) => ({
  [`& .${gridClasses.columnHeaderTitle}`]: {
    fontWeight: 'bold'
  },
  [`& .${gridClasses.row}.odd`]: {
    backgroundColor: theme.palette.grey[200],
    '&:hover, &.Mui-hovered': {
      backgroundColor: alpha(theme.palette.primary.main, ODD_OPACITY),
      '@media (hover: none)': {
        backgroundColor: 'transparent',
      },
    },
    '&.Mui-selected': {
      backgroundColor: alpha(
        theme.palette.primary.main,
        ODD_OPACITY + theme.palette.action.selectedOpacity,
      ),
      '&:hover, &.Mui-hovered': {
        backgroundColor: alpha(
          theme.palette.primary.main,
          ODD_OPACITY +
            theme.palette.action.selectedOpacity +
            theme.palette.action.hoverOpacity,
        ),
        // Reset on touch devices, it doesn't add specificity
        '@media (hover: none)': {
          backgroundColor: alpha(
            theme.palette.primary.main,
            ODD_OPACITY + theme.palette.action.selectedOpacity,
          ),
        },
      },
    },
  },
}));

export default function DataGridDemo({rows, columns, pageSize}) {
  return (
    <div style={{ height: 400, width: '100%' }}>
      <DataGrid
        rows={rows}
        columns={columns}
        pageSize={10}
        rowsPerPageOptions={[5]}
      />
    </div>
  );
}

export function StripedGrid({rows, columns, loading}) {
  return (
    <div style={{ height: 400, width: '100%' }}>
      <StripedDataGrid
        loading={loading}
        rows={rows}
        columns={columns}
        pageSize={10}
        rowsPerPageOptions={[5]}
        getRowClassName={(params) =>
          params.indexRelativeToCurrentPage % 2 === 0 ? 'even' : 'odd'
        }
        localeText={esES.components.MuiDataGrid.defaultProps.localeText}
        components={{
          Toolbar: GridToolbar,
          LoadingOverlay: LinearProgress
        }}
        filterMode="server"
        disableColumnFilter
      />
    </div>
  );
}