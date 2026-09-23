const config = {
    // basename: only at build time to set, and Don't add '/' at end off BASENAME for breadcrumbs, also Don't put only '/' use blank('') instead,
    // like '/berry-material-react/react/default'
    basename: '',
    defaultPath: '/',
    fontFamily: `'Roboto', sans-serif`,
    borderRadius: 12,
    stsAuthority: "https://silappauth.azurewebsites.net/",
    clientId: process.env.REACT_APP_CLIENTID,
    clientScope: "openid profile",
    clientRoot: process.env.REACT_APP_CLIENTROOT
};

export const getApiUrl = path => {
    let url = ''
    switch(process.env.NODE_ENV) {
        case 'production':
            url = `https://silapp.azurewebsites.net${path}`;
            break;
        case 'development':
        default:
            url = `https://localhost:7097${path}`;
    }
    return url
}

export const getAuthUrl = path => {
    let url = ''
    switch(process.env.NODE_ENV) {
        case 'production':
            url = '';
            break;
        case 'development':
        default:
            url = `https://localhost:44303${path}`;
    }
    return url
}

export default config;
