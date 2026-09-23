const storeUser = (user) => {
  window.localStorage.setItem("user", JSON.stringify(user))
}

const getStoredUser = () => {
  return JSON.parse(window.localStorage.getItem("user"))
}

const clearStoredUser = () => {
  window.localStorage.clear()
}

const storeToken = (token) => {
  window.localStorage.setItem("token", token)
}

const getStoredToken = () => {
  return window.localStorage.getItem("token")
}

const clearStoredToken = () => {
  window.localStorage.clear()
}

export { storeUser, getStoredUser, clearStoredUser, storeToken, getStoredToken, clearStoredToken }