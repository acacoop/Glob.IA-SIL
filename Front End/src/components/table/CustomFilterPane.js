import * as React from 'react';
import { Button } from '@mui/material';
import { useGridSelector, GridApiContext, GridAddIcon, GridPanelContent, GridPanelFooter, GridPanelWrapper, GridFilterForm } from '@mui/x-data-grid';
//import { optionsSelector } from '../../../hooks/utils/optionsSelector';
//import { GridApiContext } from '../../GridApiContext';
//import { GridAddIcon } from '../../icons/index';
//import { GridPanelContent } from '../GridPanelContent';
//import { GridPanelFooter } from '../GridPanelFooter';
//import { GridPanelWrapper } from '../GridPanelWrapper';
//import { GridFilterForm } from './GridFilterForm';

export default function CustomFilterPane() {
  const apiRef = React.useContext(GridApiContext);
  const [gridState] = apiRef.current.state;
  const { disableMultipleColumnsFiltering } = useGridSelector(apiRef);

  const hasMultipleFilters = React.useMemo(() => gridState.filter.items.length > 1, [
    gridState.filter.items.length,
  ]);

  const applyFilter = React.useCallback(
    (item) => {
      apiRef.current.upsertFilter(item);
    },
    [apiRef],
  );

  const applyFilterLinkOperator = React.useCallback(
    (operator) => {
      apiRef.current.applyFilterLinkOperator(operator);
    },
    [apiRef],
  );

  const addNewFilter = React.useCallback(() => {
    apiRef.current.upsertFilter({});
  }, [apiRef]);

  const deleteFilter = React.useCallback(
    (item) => {
      apiRef.current.deleteFilter(item);
    },
    [apiRef],
  );

  React.useEffect(() => {
    if (gridState.filter.items.length === 0) {
      addNewFilter();
    }
  }, [addNewFilter, gridState.filter.items.length]);

  return (
    <GridPanelWrapper>
      <GridPanelContent>
        {gridState.filter.items.map((item, index) => (
          <GridFilterForm
            key={item.id}
            item={item}
            applyFilterChanges={applyFilter}
            deleteFilter={deleteFilter}
            hasMultipleFilters={hasMultipleFilters}
            showMultiFilterOperators={index > 0}
            multiFilterOperator={gridState.filter.linkOperator}
            disableMultiFilterOperator={index !== 1}
            applyMultiFilterOperatorChanges={applyFilterLinkOperator}
          />
        ))}
      </GridPanelContent>
      {!disableMultipleColumnsFiltering && (
        <GridPanelFooter>
          <Button onClick={addNewFilter} startIcon={<GridAddIcon />} color="primary">
            {apiRef.current.getLocaleText('filterPanelAddFilter')}
          </Button>
        </GridPanelFooter>
      )}
    </GridPanelWrapper>
  );
}