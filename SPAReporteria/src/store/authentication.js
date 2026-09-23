import { createSlice } from '@reduxjs/toolkit'

export const customization = createSlice({
  name: 'authentication',
  initialState: {
    isLoggedIn: false
  },
  reducers: {
    signIn: (state, action) => {
      const id = action.payload.id;
      return {
          ...state,
          isOpen: [id]
      }
    },
    isLoggedIn: (state) => {
      return {
        isLoggedIn: state.isLoggedIn
      }
    }
  },
})

// Action creators are generated for each case reducer function
export const { menuOpen, setMenu } = customization.actions

export default customization.reducer