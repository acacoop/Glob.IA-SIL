const config = {
  basename: "",
  defaultPath: "/",
  fontFamily: `'Roboto', sans-serif`,
  borderRadius: 12,
  stsAuthority: process.env.REACT_APP_AUTHORITY,
  clientId: process.env.REACT_APP_CLIENTID,
  clientScope: "openid profile",
  clientRoot: process.env.REACT_APP_CLIENTROOT,
  //silData: process.env.REACT_APP_LOCALAPISILDATA
};

export const getApiUrlComunicaciones = (path) => {
  return `${process.env.REACT_APP_APICOMUNICACIONES}${path}`;
};

export const getApiUrlReporteria = (path) => {
  return `${process.env.REACT_APP_APIREPORTERIA}${path}`;
};

export const getApiUrlData = (path) => {
  return `${process.env.REACT_APP_APIDATA}${path}`;
};

export const getAuthUrl = (path) => {
  let url = "";
  switch (process.env.NODE_ENV) {
    case "production":
      console.log("API:", process.env.REACT_APP_APIREPORTERIA);
      url = "";
      break;
    case "development":
      console.log("API:", process.env.REACT_APP_APIREPORTERIA);
    default:
      url = `https://localhost:44303${path}`;
      console.log("API:", process.env.REACT_APP_APIREPORTERIA);
  }
  return url;
};
export const getApiSilData = (path) => {
  let url = "";
  switch (process.env.NODE_ENV) {
    case "production":
      console.log("API:", process.env.REACT_APP_APISILDATA);
      url = "";
      break;
    case "development":
      console.log("API:", process.env.REACT_APP_LOCALAPISILDATA);
      url = `${process.env.REACT_APP_LOCALAPISILDATA}${path}`;
      break;
    default:
      console.log("API:", process.env.REACT_APP_LOCALAPISILDATA);
      url = `https://localhost:7024/${path}`;
  }
  return url;
};
export const getApiUrlReporteriaLocal = (path) => {
  return `${process.env.REACT_APP_APIREPORTERIALOCAL}${path}`;
};
export default config;
