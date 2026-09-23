// react
import { useState } from 'react'
//vimport { useEffect, useState } from "react";
import useApi from 'hooks/useApi'
import { getAllContactsLists, getContactsLists, removeContactList } from 'services/contactlist'
import searchApi from 'utils/search'

// components
import MainCard from "components/cards/MainCard"
import FormInputTextSearch from "components/input/FormInputTextSearch"
import { FormButtonLink } from "components/button";
import { StripedGrid } from 'components/table/CustomDataGrid'
import Confirm from 'components/dialog/Confirm'

// mui
import { Grid } from '@mui/material'
import AddIcon from '@mui/icons-material/Add';
import { GridActionsCellItem } from '@mui/x-data-grid'
import DeleteIcon from '@mui/icons-material/Delete';
import EditIcon from '@mui/icons-material/Edit';
import { useTheme } from '@mui/material/styles'

import { BrowserView, MobileView } from 'react-device-detect';
import { useNavigate } from 'react-router-dom'
import { AlertResponse } from "components/alert"

export default function ContactsList() {
  const { isLoading, data, setData } = useApi(getAllContactsLists)
  const [search, setSearch] = useState()
  const [searchLoading, setSearchLoading] = useState(false)
  const [openModal, setOpenModal] = useState(0)
  const [apiResponse, setApiResponse] = useState({})
  const theme = useTheme()
  const navigate = useNavigate()

  const handleChange = e => {
    setSearch(e.currentTarget.value)
    searchApi(e.currentTarget.value, 1, setSearch, setData, getAllContactsLists, getContactsLists, setSearchLoading)
  }

  const columns = {
    browser: [
      { field: 'nombre', headerName: 'Nombre', flex: 1, sortable: false },
      { field: 'descripcion', headerName: 'Descripcion', flex: 1, sortable: false },
      {
        field: 'actions',
        type: 'actions',
        width: 80,
        getActions: (params) => [
          <GridActionsCellItem
            icon={<EditIcon />}
            label="Editar"
            sx={{
              color: theme.palette.secondary.dark
            }} 
            onClick={() => {
              navigate(params.row.id.toString())}
            } />,
          <GridActionsCellItem
            icon={<DeleteIcon />}
            label="Borrar"
            sx={{
              color: theme.palette.error.dark
            }}
            onClick={() => handleRemove(params.id)}
          />,
        ],
      },
    ],
    mobile: [
      { field: 'nombre', headerName: 'Nombre', flex: 1, sortable: false },
      {
        field: 'actions',
        type: 'actions',
        width: 80,
        getActions: (params) => [
          <GridActionsCellItem
            icon={<EditIcon />}
            label="Editar"
            sx={{
              color: theme.palette.secondary.dark
            }} 
            onClick={() => {
              navigate(params.row.id.toString())}
            } />,
          <GridActionsCellItem
            icon={<DeleteIcon />}
            label="Borrar"
            sx={{
              color: theme.palette.error.dark
            }}
            onClick={() => handleRemove(params.id)}
          />,
        ],
      },
    ]
  };

  const handleRemove = id => {
    setOpenModal(id)
  }

  const handleClose = () => {
    setOpenModal(0)
  }

  const handleAgree = () => {
    removeContactList(openModal).then(response => {
      setApiResponse(response)
      if (response?.typeOfBusinessRule === undefined) {
        setData(oldData => oldData.filter(data => data.id !== openModal))
      }
    })
    setOpenModal(0)
  }

  return <div>
    <MainCard title="Listas de Contactos">
      <Grid container>
        <Grid container spacing={2}>
          <Grid item md={6} sm={9} sx={{ marginBottom: 3 }}>
            <FormInputTextSearch value={search} onChange={handleChange} others={{ fullWidth: true }}/>
          </Grid>
          <Grid item md={6} sm={9} sx={{ marginBottom: 3, textAlign: "right", alignSelf: "center" }}>
            <FormButtonLink to="nuevo">
              <AddIcon />
              Agregar
            </FormButtonLink>
          </Grid>
        </Grid>
        <Grid item xs={12}>
          <BrowserView>
            <StripedGrid rows={data} columns={columns.browser} loading={isLoading || searchLoading} />
          </BrowserView>
          <MobileView>
            <StripedGrid rows={data} columns={columns.mobile} loading={isLoading || searchLoading} />
          </MobileView>
        </Grid>
      </Grid>
    </MainCard>
    <AlertResponse response={apiResponse} successMessage={"Se borró correctamente"} />
    <Confirm title={"Esta seguro?"} open={openModal !== 0} handleAgree={handleAgree} handleClose={handleClose} />
  </div>
}