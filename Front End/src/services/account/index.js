import get from '../serviceApi'

export const getAllAccounts = () => {
  return get("/api/")
}