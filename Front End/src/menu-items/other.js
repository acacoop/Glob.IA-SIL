// assets
import GoogleIcon from '@mui/icons-material/Google';
import HelpOutlineIcon from '@mui/icons-material/HelpOutline';

// constant
const icons = { GoogleIcon, HelpOutlineIcon };

// ==============================|| SAMPLE PAGE & DOCUMENTATION MENU ITEMS ||============================== //

const other = {
    id: 'sample-docs-roadmap',
    type: 'group',
    children: [
        {
            id: 'sample-page',
            title: 'Sample Page',
            type: 'item',
            url: '/sample-page',
            icon: icons.GoogleIcon,
            breadcrumbs: false
        },
        {
            id: 'documentation',
            title: 'Documentation',
            type: 'item',
            url: 'https://codedthemes.gitbook.io/berry/',
            icon: icons.HelpOutlineIcon,
            external: true,
            target: true
        }
    ]
};

export default other;
