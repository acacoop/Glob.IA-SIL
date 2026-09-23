import { Routes, Route } from "react-router-dom";

import { lazy } from "react";

import MainLayout from "layout/MainLayout";
import MinimalLayout from "layout/MinimalLayout";
import Loadable from "components/load/Loadable";
import Home from "views/home";
import ProtectedRoute from "./ProtectedRoute";
import Index from "views/Inicio/index";

const AuthLogin3 = Loadable(
  lazy(() => import("views/pages/authentication/authentication3/Login3"))
);
const SignIn = Loadable(
  lazy(() => import("views/pages/authentication/authentication3/SignIn"))
);
const CPE = Loadable(lazy(() => import("views/reports/cpe")));
const PanelControlLogistico = Loadable(
  lazy(() => import("views/Inicio/index"))
);

// ==============================|| ROUTING RENDER ||============================== //

export default function ThemeRoutes() {
  return (
    <Routes>
      <Route
        path="/"
        element={
          <ProtectedRoute>
            <MainLayout />
          </ProtectedRoute>
        }
      >
        <Route index element={<PanelControlLogistico />} />
        <Route
          path="panelControlLogistico"
          element={<PanelControlLogistico />}
        />
      </Route>
      <Route
        path="/"
        element={
          <ProtectedRoute>
            <MainLayout />
          </ProtectedRoute>
        }
      >
        <Route index element={<CPE />} />
        <Route path="reportes/reporte-cpe" element={<CPE />} />
      </Route>
      <Route path="/" element={<MinimalLayout />}>
        <Route path="login" element={<AuthLogin3 />} />
        <Route path="signin-callback" element={<SignIn />} />
      </Route>
    </Routes>
  );
}
