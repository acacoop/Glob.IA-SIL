import React from "react";
import ReactDOM from "react-dom/client";
import "./index.css";
import App from "./App";
import reportWebVitals from "./reportWebVitals";
import AuthProvider from "providers/AuthProvider";
import { getStoredUser } from "services/auth/UserStore";

// third party
import { BrowserRouter } from "react-router-dom";
import { Provider } from "react-redux";

import { store } from "store";

// style + assets
import "assets/scss/style.scss";
import "antd/dist/reset.css"; // para Ant Design v5

const root = ReactDOM.createRoot(document.getElementById("root"));
root.render(
  <Provider store={store}>
    <AuthProvider storedUser={getStoredUser()}>
      <BrowserRouter>
        <App />
      </BrowserRouter>
    </AuthProvider>
  </Provider>
);

// If you want to start measuring performance in your app, pass a function
// to log results (for example: reportWebVitals(console.log))
// or send to an analytics endpoint. Learn more: https://bit.ly/CRA-vitals
reportWebVitals();
