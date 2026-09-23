import PropTypes from 'prop-types';

// material-ui
import { useTheme } from '@mui/material/styles';
import { Avatar, Box, ButtonBase, AppBar, Toolbar } from '@mui/material';

// project imports
import LogoSection from 'layout/MainLayout/LogoSection';
import ProfileSection from 'layout/MainLayout/Header/ProfileSection';

// assets
import MenuIcon from '@mui/icons-material/Menu';

// ==============================|| MAIN NAVBAR / HEADER ||============================== //
const Upbar = ({ leftDrawerOpened, handleLeftDrawerToggle }) => {
    const theme = useTheme();
    return (
        <AppBar
            enableColorOnDark
            position="fixed"
            color="inherit"
            elevation={0}
            sx={{
                bgcolor: theme.palette.primary.dark,
                transition: leftDrawerOpened ? theme.transitions.create('width') : 'none'
            }}
        >
            <Toolbar>
                <Header handleLeftDrawerToggle={handleLeftDrawerToggle} />
            </Toolbar>
        </AppBar>
    );
};

const Header = ({ handleLeftDrawerToggle }) => {
    const theme = useTheme();

    return (
        <>
            {/* logo & toggler button */}
            <Box
                sx={{
                    width: 228,
                    display: 'flex',
                    [theme.breakpoints.down('md')]: {
                        width: 'auto'
                    }
                }}
            >
                <Box component="span" sx={{ display: { xs: 'none', md: 'block' }, flexGrow: 1 }}>
                    <LogoSection />
                </Box>
                <ButtonBase sx={{ borderRadius: '12px', overflow: 'hidden' }}>
                    <Avatar
                        variant="rounded"
                        sx={{
                            ...theme.typography.commonAvatar,
                            ...theme.typography.mediumAvatar,
                            transition: 'all .2s ease-in-out',
                            background: theme.palette.primary.dark,
                            border: `1px solid ${theme.palette.primary.text.colourful}`,
                            color: theme.palette.primary.text.colourful,
                            '&:hover': {
                                background: theme.palette.secondary.dark,
                                color: theme.palette.secondary.light
                            }
                        }}
                        onClick={handleLeftDrawerToggle}
                        color="inherit"
                    >
                        <MenuIcon stroke={1.5} size="1.3rem" />
                    </Avatar>
                </ButtonBase>
            </Box>

            {/* header search */}
            <Box sx={{ flexGrow: 1 }}>
                <h1 style={{padding: "0 1rem", color: theme.palette.primary.text.colourful}}>Comunicaciones</h1>
            </Box>
            <Box sx={{ flexGrow: 1 }} />

            {/* notification & profile */}
            <ProfileSection />
        </>
    );
};

Upbar.propTypes = {
    handleLeftDrawerToggle: PropTypes.func
};

Header.propTypes = {
    handleLeftDrawerToggle: PropTypes.func
};

export default Header;
export { Upbar };
