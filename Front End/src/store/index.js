import { configureStore  } from '@reduxjs/toolkit';
import customizationReducer from './customization';

// ==============================|| REDUX - MAIN STORE ||============================== //

const store = configureStore({
  reducer: {
    customization: customizationReducer,
  },
});

const persister = 'Free';

export { store, persister };
