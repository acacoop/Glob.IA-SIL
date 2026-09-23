// assets
import ContactsIcon from '@mui/icons-material/Contacts';
import ListAltIcon from '@mui/icons-material/ListAlt';

// constant
const icons = { ContactsIcon, ListAltIcon };

const contacts = {
  id: 'contacts',
  title: 'Contactos',
  type: 'group',
  children: [
    {
        id: 'page-contacts',
        title: 'Contactos',
        type: 'item',
        url: '/contactos',
        icon: icons.ContactsIcon,
        breadcrumbs: false
    },
    {
      id: 'page-contacts-list',
      title: 'Listas',
      type: 'item',
      url: '/listas-contactos',
      icon: icons.ListAltIcon,
      breadcrumbs: false
    }
  ]
};

export default contacts;