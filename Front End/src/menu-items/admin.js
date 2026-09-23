// assets
import AssignmentIndIcon from '@mui/icons-material/AssignmentInd';
import PublicIcon from '@mui/icons-material/Public';
import CorporateFareIcon from '@mui/icons-material/CorporateFare';
import AccountTreeIcon from '@mui/icons-material/AccountTree';
import OutboxIcon from '@mui/icons-material/Outbox';

const contacts = {
  id: 'admin',
  title: 'Administración',
  type: 'group',
  children: [
    {
        id: 'page-rol',
        title: 'Roles',
        type: 'item',
        url: '/roles',
        icon: AssignmentIndIcon,
        breadcrumbs: false,
        disabled: true
    },
    {
        id: 'page-zone',
        title: 'Zonas Comerciales',
        type: 'item',
        url: '/zonas',
        icon: PublicIcon,
        breadcrumbs: false,
        disabled: true
    },
    {
      id: 'page-centers',
      title: 'Centros',
      type: 'item',
      url: '/centros',
      icon: CorporateFareIcon,
      breadcrumbs: false,
      disabled: true
    },
    {
      id: 'page-accounts',
      title: 'Cuentas',
      type: 'item',
      url: '/cuentas',
      icon: AccountTreeIcon,
      breadcrumbs: false,
      disabled: true
    },
    {
      id: 'page-type-contacts',
      title: 'Tipos de Contactos',
      type: 'item',
      url: '/tipos-contactos',
      icon: OutboxIcon,
      breadcrumbs: false,
      disabled: true
    }
  ]
};

export default contacts;