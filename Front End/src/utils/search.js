let touch;
  
const search = (value, page, setSearch, setData, getAll, getFilter, setLoading) => {
  setSearch(value)
  clearTimeout(touch)
  touch = setTimeout(() => {  
    searchFunction(value, page, setData, getAll, getFilter, setLoading)
  }, 300)
}

const searchFunction = (value, page, setData, getAll, getFilter, setLoading) => {
  if(value && value.length > 0) {
    setLoading(true)
    getFilter(value, page).then(persons => {
      setData(persons)
      setLoading(false)
    })
  } else {
    setLoading(true)
    getAll().then(persons => {
      setData(persons)
      setLoading(false)
    })
  }
}

export default search