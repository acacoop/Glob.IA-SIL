// assets
import TextFormatIcon from '@mui/icons-material/TextFormat';
import PaletteIcon from '@mui/icons-material/Palette';
import BrightnessMediumIcon from '@mui/icons-material/BrightnessMedium';
import FormatPaintIcon from '@mui/icons-material/FormatPaint';

// constant
const icons = {
    TextFormatIcon,
    PaletteIcon,
    BrightnessMediumIcon,
    FormatPaintIcon
};

// ==============================|| UTILITIES MENU ITEMS ||============================== //

const utilities = {
    id: 'utilities',
    title: 'Utilities',
    type: 'group',
    children: [
        {
            id: 'util-typography',
            title: 'Typography',
            type: 'item',
            url: '/utils/util-typography',
            icon: icons.TextFormatIcon,
            breadcrumbs: false
        },
        {
            id: 'util-color',
            title: 'Color',
            type: 'item',
            url: '/utils/util-color',
            icon: icons.PaletteIcon,
            breadcrumbs: false
        },
        {
            id: 'util-shadow',
            title: 'Shadow',
            type: 'item',
            url: '/utils/util-shadow',
            icon: icons.BrightnessMediumIcon,
            breadcrumbs: false
        },
        {
            id: 'icons',
            title: 'Icons',
            type: 'collapse',
            icon: icons.FormatPaintIcon,
            children: [
                {
                    id: 'tabler-icons',
                    title: 'Tabler Icons',
                    type: 'item',
                    url: '/icons/tabler-icons',
                    breadcrumbs: false
                },
                {
                    id: 'material-icons',
                    title: 'Material Icons',
                    type: 'item',
                    url: '/icons/material-icons',
                    breadcrumbs: false
                }
            ]
        }
    ]
};

export default utilities;
