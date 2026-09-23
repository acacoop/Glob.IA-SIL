import { StripedGrid } from 'components/table/CustomDataGrid'
import MainCard from "components/cards/MainCard";

import { ReporteCPEResponseColumns } from 'models';

const TableReport = ({isLoading = false, data = [], ...props}) => {
  const columns = {
    browser: Object.keys(ReporteCPEResponseColumns).map(key => {
      return { field: key, headerName: ReporteCPEResponseColumns[key], flex: 1, sortable: false }
    }),
    mobile: Object.keys(ReporteCPEResponseColumns).map(key => {
      return { field: key, headerName: ReporteCPEResponseColumns[key], flex: 1, sortable: false }
    })
  };

  return (
    <MainCard sx={{marginTop: '1rem'}}>
      <StripedGrid rows={data} columns={columns.browser} loading={isLoading} {...props} autoHeight={true} isRowSelectable={row => false} />
    </MainCard>
  )
}

export default TableReport;