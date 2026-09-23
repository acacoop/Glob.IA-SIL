import { useState, useEffect } from 'react'
import useApi from "hooks/useApi";
import { getAllTypes } from 'services/typeofcontact'
import { Accordion, AccordionDetails, AccordionSummary, Typography, Grid, useTheme } from '@mui/material';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import AddIcon from '@mui/icons-material/Add';
import DeleteIcon from '@mui/icons-material/Delete';
import Confirm from 'components/dialog/Confirm'
import AccordionContactsDetails from './AccordionContactsDetails';

export default function AccordionContacts({onChange, values = []}) {
  const theme = useTheme()
  const [expanded, setExpanded] = useState(false);
  const [contacts, setContacts] = useState([]);
  const [openModalRemove, setOpenModalRemove] = useState(false)
  const { isLoading, data } = useApi(getAllTypes)

  useEffect(() => {
    setContacts(values)
  }, [])

  useEffect(() => {
    onChange && onChange(contacts)
  }, [contacts])

  const handleChange = (panel) => (event, isExpanded) => {
    setExpanded(isExpanded ? panel : false);
  };

  const handleConfirmContact = (contact) => {
    setContacts(oldArray => {
      return [...oldArray, contact]
    })
  }

  const handleModifyContact = (contact) => {
    setContacts(oldArray => {
      return oldArray.map(oldContact => 
        oldContact.id === contact.id ? 
          contact : 
          oldContact
      )
    })
    setExpanded(false)
  }

  const handleRemoveContact = (id) => (event) => {
    event.stopPropagation();
    setOpenModalRemove(id)
  }

  const handleCloseModalRemove = () => {
    setOpenModalRemove(false)
  }

  const handleAgreeModalRemove = () => {
    setContacts(oldContacts => {
      return oldContacts.filter(oldContact => oldContact.id !== openModalRemove)
    })
    setOpenModalRemove(false)
  }

  return (
    <div style={{borderColor: theme.palette.grey[200], borderStyle: 'solid', borderSize: '1px', borderRadius: '4px', marginBottom: '1em'}}>
      {
        contacts.map(contact => {
          return (
            <Accordion expanded={expanded === contact.id} onChange={handleChange(contact.id)} key={contact.id}>
              <AccordionSummary
                expandIcon={<ExpandMoreIcon />}
                aria-controls={`${contact.id}bh-content`}
                id={`${contact.id}bh-header`}
              >
                <Grid container sx={{justifyContent: "space-between", alignItems: "center"}}>
                  <Grid item xs={6}>
                    <Grid container>
                      <Grid item xs={6}>
                        <Typography>
                          {contact.tipo.obj.nombre}
                        </Typography>
                      </Grid>
                      <Grid item xs={6}>
                        <Typography sx={{ color: 'text.secondary' }}>{contact.dato}</Typography>
                      </Grid>
                    </Grid>
                  </Grid>
                  <Grid item sx={{paddingLeft: "1rem", paddingRight: "1rem"}}>
                    <DeleteIcon onClick={handleRemoveContact(contact.id)} />
                  </Grid>
                </Grid>
              </AccordionSummary>
              <AccordionDetails>
                <AccordionContactsDetails 
                  dropdownData={isLoading ? [] : data.map(tipo => { return { value: JSON.stringify(tipo), label: tipo.nombre } })} 
                  contact={contact} 
                  onCheckButton={contact => handleModifyContact(contact)} 
                />
              </AccordionDetails>
            </Accordion>
          )
        })
      }
      <Accordion expanded={expanded === 'panelContactNew'} onChange={handleChange('panelContactNew')} key={'panelContactNew'} sx={{backgroundColor: theme.palette.grey[100]}}>
        <AccordionSummary
          expandIcon={<AddIcon />}
          aria-controls="panelContactNewbh-content"
          id="panelContactNewbh-header"
        >
          <Typography sx={{ flex: 1, textAlign: "center" }}>
            Agregar Contacto
          </Typography>
          <Typography sx={{ color: 'text.secondary' }}></Typography>
        </AccordionSummary>
        <AccordionDetails>
          <AccordionContactsDetails 
            dropdownData={isLoading ? [] : data.map(tipo => { return { value: JSON.stringify(tipo), label: tipo.nombre } })} 
            onCheckButton={contact => handleConfirmContact(contact)}
            contact={{}}
          />
        </AccordionDetails>
      </Accordion>
      <Confirm message={"¿Está seguro de borrar el contacto?"} open={openModalRemove ? true : false} handleAgree={handleAgreeModalRemove} handleClose={handleCloseModalRemove} />
    </div>
  )
}