import handleResponse from 'routes/handleResponse'
import { getStoredUser } from 'services/auth/UserStore'

export default function get(uri, params = []) {
  let url = ""
  if (params.length > 0) {
    url = `${uri}?${params.map(param => `${param.key}=${param.value}`).join('&')}`
  } else {
    url = `${uri}`
  }
  return fetch(url, { 
      method: "GET", 
      headers: {
        'Authorization': `Bearer ${getStoredUser().access_token}`
      }
    })
    .then(response => {
      return handleResponse(response)
    })
}

export function add(uri, data) {
  return fetch(`${uri}`, { 
    method: "POST",
    headers: {
      'Authorization': `Bearer ${getStoredUser().access_token}`,
      'Accept': 'application/json, text/plain',
      'Content-Type': 'application/json;charset=UTF-8'
    },
    body: JSON.stringify(data) 
  }).then(response => {
    return handleResponse(response)
  })
}

export function edit(uri, data) {
  return fetch(`${uri}`, { 
    method: "PUT",
    headers: {
      'Authorization': `Bearer ${getStoredUser().access_token}`,
      'Accept': 'application/json, text/plain',
      'Content-Type': 'application/json;charset=UTF-8'
    },
    body: JSON.stringify(data)
  }).then(response => {
    return handleResponse(response)
  })
}

export function remove(uri, id) {
  return fetch(`${uri}/${id}`, { 
    method: "DELETE", 
    headers: {
      'Authorization': `Bearer ${getStoredUser().access_token}`
    }})
      .then(response => {
        return handleResponse(response)
      })
}

//const handleResponse = (response) => {
//  if (response.status === 401) {
//    console.log(401)
//    AuthService.getInstance().sessionStatus().then(user => {
//      console.log(user)
//    })
//    //AuthService.getInstance().login()
//  } else {
//    return response.json()
//  }
//}