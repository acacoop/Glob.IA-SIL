import AuthService from 'services/auth/AuthService'
import { useNavigate } from 'react-router-dom'

export default function handleResponse(response) {
  if (response.status === 401) {
    AuthService.getInstance().login()
  } else {
    return response.json();
  }
}