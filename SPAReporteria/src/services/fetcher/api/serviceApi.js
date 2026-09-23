import { getStoredUser } from "services/auth/UserStore";
import handleResponse from "routes/handleResponse";

export default function get(uri, params = []) {
  let url = "";
  if (params.length > 0) {
    url = `${uri}?${params
      .map((param) => `${param.key}=${param.value}`)
      .join("&")}`;
  } else {
    url = `${uri}`;
  }
  return fetch(url, {
    method: "GET",
    headers: {
      "Content-type": "application/json",
      //'Authorization': `Bearer ${getStoredUser().access_token}`,
    },
  })
    .then((response) => {
      return handleResponse(response);
    })
    .catch((e) => console.log(e));
}

export function add(uri, data) {
  return fetch(`${uri}`, {
    method: "POST",
    headers: {
      Accept: "application/json, text/plain",
      "Content-Type": "application/json;charset=UTF-8",
      //Authorization: `Bearer ${getStoredUser().access_token}`,
    },
    body: JSON.stringify(data),
  })
    .then((response) => {
      return handleResponse(response);
    })
    .catch((e) => console.log(e));
}

export function edit(uri, data) {
  return fetch(`${uri}`, {
    method: "PUT",
    headers: {
      Accept: "application/json, text/plain",
      "Content-Type": "application/json;charset=UTF-8",
      //Authorization: `Bearer ${getStoredUser().access_token}`,
    },
    body: JSON.stringify(data),
  })
    .then((response) => {
      return handleResponse(response);
    })
    .catch((e) => console.log(e));
}

export function remove(uri, id) {
  return fetch(`${uri}/${id}`, {
    method: "DELETE",
    headers: {
      "Content-type": "application/json",
      //Authorization: `Bearer ${getStoredUser().access_token}`,
    },
  })
    .then((response) => {
      return handleResponse(response);
    })
    .catch((e) => console.log(e));
}

export async function downloadExcel(uri, data) {
  try {
    const res = await fetch(uri, {
      method: "POST",
      headers: {
        Accept:
          "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "Content-Type": "application/json;charset=UTF-8",
        // Authorization: `Bearer ${getStoredUser().access_token}`,
      },
      body: JSON.stringify(data),
    });
    debugger;
    // Verificar si la respuesta no fue exitosa
    if (!res.ok) {
      // Puede que el backend mande un JSON con mensaje de error
      const contentType = res.headers.get("Content-Type") || "";

      if (contentType.includes("application/json")) {
        const jsonError = await res.json();
        throw new Error(
          jsonError.message || jsonError.error || jsonError.Warnings || "Error desconocido"
        );
      } else {
        throw new Error(`Error HTTP: ${res.status}`);
      }
    }

    // 🔔 WARNINGS
    const warningsHeader = res.headers.get("X-Warnings");
    const recordCount = res.headers.get("X-Record-Count");

    let warningMessages = [];
    if (warningsHeader) {
      warningMessages = warningsHeader.split(" | ");
    }

    // 👀 Verificar tipo de contenido devuelto
    const contentType = res.headers.get("Content-Type") || "";

    if (contentType.includes("application/json")) {
      // En caso de que el back devuelva JSON (mensaje de error o validación)
      const json = await res.json();
      throw new Error(
        json.message || json.error || "Error al generar el Excel"
      );
    }

    // 🔍 Obtener el header de Content-Disposition
    const contentDisposition = res.headers.get("Content-Disposition");
    let fileName = "reporte.xlsx";

    if (contentDisposition) {
      // Extrae el nombre del archivo, manejando UTF-8 y formatos mixtos
      const match = contentDisposition.match(
        /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/
      );
      if (match && match[1]) {
        fileName = match[1]
          .replace(/UTF-8''/, "") // elimina UTF-8''
          .replace(/['"]/g, "") // elimina comillas
          .trim(); // limpia espacios
      }

      // 🔹 Si hay un ";" extra (como filename.xlsx; filename_=UTF-8''filename)
      if (fileName.includes(";")) {
        fileName = fileName.split(";")[0];
      }
    }

    // 🧾 Descargar el blob
    const blob = await res.blob();
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = fileName;
    document.body.appendChild(a);
    a.click();
    a.remove();
    window.URL.revokeObjectURL(url);

    return { 
      success: true, 
      warnings: warningMessages,
      recordCount: recordCount ? parseInt(recordCount) : null
    };
  } catch (e) {
    // 👇 Devolvés el error con su mensaje
    return { 
      success: false, 
      message: e.message 
    };
  }
}

// export function downloadExcel(uri, data) {
//   return fetch(`${uri}`, {
//     method: "POST",
//     headers: {
//       Accept:
//         "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
//       "Content-Type": "application/json;charset=UTF-8",
//       //Authorization: `Bearer ${getStoredUser().access_token}`,
//     },
//     body: JSON.stringify(data),
//   })
//     .then(async (res) => {
//       const blob = await res.blob();
//       const url = window.URL.createObjectURL(blob);
//       const a = document.createElement("a");
//       a.href = url;
//       a.download = res.headers
//         .get("Content-Disposition")
//         .split(";")[1]
//         .replace("filename=", "")
//         .trim();
//       document.body.appendChild(a); // we need to append the element to the dom -> otherwise it will not work in firefox
//       a.click();
//       a.remove();
//     })
//     .catch((e) => console.log("Error al exportar CPE", e));
// }
