import { useState, useEffect } from 'react'
import { Grid, useTheme } from '@mui/material';
import FormInputText from 'components/input/FormInputText';
import { FormIconButton } from 'components/button';
import CheckIcon from '@mui/icons-material/Check';
import ClearIcon from '@mui/icons-material/Clear';
import FormSelect from "components/input/FormSelect"

export default function AccordionContactsDetails({dropdownData, onCheckButton, contact}) {
  const theme = useTheme()
  const [oldContact, setOldContact] = useState({});
  const [editableContact, setEditableContact] = useState({});
  const [validationsInput, setValidationsInput] = useState({
    inputText: {
      touched: false,
      isEmpty: true
    },
    dropdown:  {
      touched: false,
      isEmpty: true
    }
  });

  useEffect(() => {
    setOldContact(contact || {})
    setEditableContact(contact || {})
    setValidationsInput(contact ? 
      {
        inputText: {
          touched: false,
          isEmpty: false
        },
        dropdown:  {
          touched: false,
          isEmpty: false
        }
      } : 
      {
        inputText: {
          touched: false,
          isEmpty: true
        },
        dropdown:  {
          touched: false,
          isEmpty: true
        }
      })
  }, [contact])

  const newContact = (oldContact, contact = {}) => {
    return { id: oldContact.id ?? Math.random(), ...oldContact, ...contact }
  }

  const handleContactData = (event) => {
    setEditableContact(oldContact => { return newContact(oldContact, { dato: event.target.value }) })
    setValidationsInput(lastEvent => {
      return {
        ...lastEvent,
        inputText:  {
          touched: true,
          isEmpty: event.target.value.trim() === ""
        }
      } 
    })
  }

  const handleSelectedType = (event) => {
    setEditableContact(oldContact => { return newContact(oldContact, { tipo: { id: event.target.value, obj: JSON.parse(event.target.value) } }) })
    setValidationsInput(lastEvent => {
      return {
        ...lastEvent,
        dropdown:  {
          touched: true,
          isEmpty: event.target.value.trim() === ""
        }
      } 
    })
  }

  const handleResetContact = (event) => {
    setEditableContact(newContact(oldContact))
    setValidationsInput({
      inputText: {
        touched: false,
        isEmpty: true
      },
      dropdown:  {
        touched: false,
        isEmpty: true
      }
    })
  }

  const handleCheckbutton = (event) => {
    onCheckButton(editableContact)
  }

  const handleValidationsInput = (element) => (e) => {
    setValidationsInput(lastEvent => {
      return { 
        inputText: {
          touched: element === "inputText" || lastEvent?.inputText?.touched,
          isEmpty: element === "inputText" ? e.target.value.trim() === "" : lastEvent?.inputText?.isEmpty
        },
        dropdown:  {
          touched: element === "dropdown" || lastEvent?.dropdown?.touched,
          isEmpty: element === "dropdown" ? e.target.value.trim() === "" : lastEvent?.dropdown?.isEmpty
        }
      } 
    })
  }

  return (
    <Grid container spacing={2}>
      <Grid item xs={12} sm={12} md={6} lg={6}>
        <FormInputText 
          label={"Contacto"} 
          value={editableContact.dato || ""} 
          onChange={handleContactData} 
          onBlur={handleValidationsInput("inputText")}
          onError={value => {
            return Boolean(validationsInput?.inputText?.touched && value.trim() === "")
          }}
          error={"Requerido"}
        />
      </Grid>
      <Grid item xs={12} sm={12} md={4} lg={4}>
        <FormSelect
          id={`TipoContacto${newContact(oldContact).id}`}
          value={editableContact.tipo?.id || ""} 
          onChange={handleSelectedType}
          items={dropdownData}
          label={"Tipo Contacto"}
          name={"tipoContacto"}
          onBlur={handleValidationsInput("dropdown")}
          onError={value => {
            return Boolean(validationsInput?.dropdown?.touched && value.trim() === "")
          }}
          error={"Requerido"}
        />
      </Grid>
      <Grid item xs={6} sm={6} md={1} lg={1} sx={{alignSelf: 'center', textAlign: 'center'}}>
        <FormIconButton onClick={handleResetContact}>
          <ClearIcon />
        </FormIconButton>
      </Grid>
      <Grid item xs={6} sm={6} md={1} lg={1} sx={{alignSelf: 'center', textAlign: 'center'}}>
        <FormIconButton color={{
          border: theme.palette.success.dark,
          color: theme.palette.success.dark,
          hover: {
            background: theme.palette.success.dark,
            color: theme.palette.success.light
          }
        }}
        disabled={Boolean(validationsInput?.inputText?.isEmpty || validationsInput?.dropdown?.isEmpty)}
        onClick={handleCheckbutton}>
          <CheckIcon />
        </FormIconButton>
      </Grid>
    </Grid>
  )
}