import config from 'config'
import { UserManager } from 'oidc-client-ts';
import { clearStoredUser } from 'services/auth/UserStore';

const settings = {
  authority: config.stsAuthority,
  client_id: config.clientId,
  redirect_uri: `${config.clientRoot}signin-callback`,
  silent_redirect_uri: `${config.clientRoot}silent-callback`,
  post_logout_redirect_uri: `${config.clientRoot}login`,
  response_type: 'code',
  scope: config.clientScope
}

export default class AuthService {
  static instance = null;
  static userManager = null;

  constructor() {
    this.userManager = new UserManager(settings);
  }

  static getInstance = () => {
    if (this.instance === null) {
      this.instance = new AuthService()
    }
    return this.instance;
  }

  get = () => {
    return this.userManager;
  }

  getUser = () => {
    return this.userManager.getUser();
  }
  
  login = () => {
    return this.userManager.signinRedirect();
    //console.log("Login")
  }
  
  renewToken = () => {
    return this.userManager.signinSilent();
  }
  
  logout = () => {
    //const idToken = getStoredUser().id_token;
    //return this.userManager.signoutRedirect({ 'id_token_hint': idToken, 'post_logout_redirect_uri': settings.post_logout_redirect_uri });
    clearStoredUser()
    return this.userManager.signoutRedirect();
  }

  sessionStatus = () => {
    return this.userManager.querySessionStatus();
  }
}