import { useSelector } from 'react-redux';

import { ThemeProvider } from '@mui/material/styles';
import { CssBaseline, StyledEngineProvider } from '@mui/material';

// routing
import Routes from 'routes';

// defaultTheme
import themes from 'themes';

// project imports
import NavigationScroll from 'layout/NavigationScroll';
import AuthProvider from 'providers/AuthProvider';
import { getStoredUser } from 'services/auth/UserStore';

// third party
import { BrowserRouter } from 'react-router-dom';

// ==============================|| APP ||============================== //

const App = () => {
    const customization = useSelector((state) => state.customization);

    return (
        <AuthProvider storedUser={getStoredUser()}>
            <BrowserRouter>
                <StyledEngineProvider injectFirst>
                    <ThemeProvider theme={themes(customization)}>
                        <CssBaseline />
                        <NavigationScroll>
                            <Routes />
                        </NavigationScroll>
                    </ThemeProvider>
                </StyledEngineProvider>
            </BrowserRouter>
        </AuthProvider>
    );
};

export default App;
