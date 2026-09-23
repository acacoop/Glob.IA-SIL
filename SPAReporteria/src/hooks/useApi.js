// react
import { useEffect, useState } from "react";

export default function useApi(getAll, id) {
  const [data, setData] = useState([])
  const [isLoading, setIsLoading] = useState(true)
  
  useEffect(() => {
    getAll(id)
      .then(data => {
        setData(data)
        setIsLoading(false)
      })
      .catch(err => {
        setData([])
        setIsLoading(false)
        console.error(err)
      })
  }, [getAll, id])

  return { isLoading, data, setData }
}