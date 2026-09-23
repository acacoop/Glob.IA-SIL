import { createContext, useState, useEffect } from 'react'
import AuthService from 'services/auth'
import { storeUser, clearStoredUser } from 'services/auth/UserStore';

export const AuthContext = createContext(null);

export default function AuthProvider({ storedUser, children }) {
  const [token, setToken] = useState(storedUser?.access_token);

  //useEffect(() => {
  //  const storedToken = getStoredToken()
  //  if (storedToken) {
  //    setToken(storedToken)
  //  }
  //  console.log(`Local Storeage ${storedToken}`)
  //}, [])

  const handleLogin = (user) => {
    setToken(user.access_token)
    storeUser(user)
  };

  const login = () => {
    AuthService.getInstance().login().catch(err => console.error(err))
  };

  const handleLogout = () => {
    AuthService.getInstance().logout()
  };

  const sessionStatus = () => {
    return AuthService.getInstance().getUser().catch(err => console.error(err))
  };

  const value = {
    token,
    login: login,
    onLogin: handleLogin,
    onLogout: handleLogout,
    sessionStatus
  };

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
};