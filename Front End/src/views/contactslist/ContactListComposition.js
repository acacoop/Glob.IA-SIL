import TreeView from '@mui/lab/TreeView';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import ChevronRightIcon from '@mui/icons-material/ChevronRight';
import TreeItem from '@mui/lab/TreeItem';

export default function ContactListComposition({ data }) {
  const renderTree = (nodes) => (
    <TreeItem key={nodes.id} nodeId={nodes.id} label={nodes.nombre}>
      {Array.isArray(nodes.contactos)
        ? nodes.contactos.map((node) => renderTree(node))
        : null}
    </TreeItem>
  );

  return (
      <TreeView
        aria-label="Composición de Contactos de la lista"
        defaultCollapseIcon={<ExpandMoreIcon />}
        defaultExpandIcon={<ChevronRightIcon />}
        sx={{ flexGrow: 1, overflowY: 'auto' }}
      >
        {renderTree(data)}
      </TreeView>
  )
}