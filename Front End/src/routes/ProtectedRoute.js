import useAuth from "hooks/useAuth";

const ProtectedRoute = ({ children }) => {
  const { token, login, sessionStatus } = useAuth();
  
  sessionStatus().then(user => {
    if(!user) login()
  })

  if (!token) {
    login()
  } else {
    return children;
  }
};

export default ProtectedRoute;