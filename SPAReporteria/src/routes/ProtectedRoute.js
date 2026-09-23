import useAuth from "hooks/useAuth";

const ProtectedRoute = ({ children }) => {
  const { token, login } = useAuth();

  if (!token) {
    login()
  } else {
    return children;
  }
  return children
};

export default ProtectedRoute;