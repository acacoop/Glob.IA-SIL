import AuthService from "services/auth/AuthService";

export default async function handleResponse(response) {
  try {
    if (response.status === 401) {
      AuthService.getInstance().login();
    } else {
      const json = await response.json(); // solo una lectura
      return json;
    }
  } catch (e) {
    return null;
  }
}
