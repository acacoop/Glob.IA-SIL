import { useState, useEffect } from 'react'
import { BusinessRule } from "models/errors"
import { SuccessAlert, ErrorAlert } from '.'

export default function AlertResponse({response, successMessage}) {
  const [showSuccessAlert, setShowSuccessAlert] = useState(false)
  const [showErrorAlert, setShowErrorAlert] = useState(BusinessRule)

  useEffect(() => {
    if (response?.typeOfBusinessRule !== undefined && response.typeOfBusinessRule >= 0) {
      setShowSuccessAlert(false)
      setShowErrorAlert(response)
    } else if (Object.keys(response).length > 0) {
      setShowErrorAlert(BusinessRule)
      setShowSuccessAlert(true)
    }
  }, [response])

  return <>
    <SuccessAlert showAlert={showSuccessAlert} message={successMessage} />
    <ErrorAlert showAlert={showErrorAlert?.typeOfBusinessRule >= 0} message={showErrorAlert?.message}/>
  </>
}