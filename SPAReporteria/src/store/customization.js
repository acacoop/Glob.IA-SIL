import { createSlice } from '@reduxjs/toolkit'

export const customization = createSlice({
  name: 'customization',
  initialState: {
    isOpen: [], // for active default menu
    opened: true
  },
  reducers: {
    menuOpen: (state, action) => {
      const id = action.payload.id;
      return {
          ...state,
          isOpen: [id]
      }
    },
    setMenu: (state, action) => {
      return {
          ...state,
          opened: action.payload.opened
      }
    }
  },
})

// Action creators are generated for each case reducer function
export const { menuOpen, setMenu } = customization.actions

export default customization.reducer