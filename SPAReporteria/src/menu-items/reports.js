// assets
import CalendarTodayIcon from "@mui/icons-material/CalendarToday";
import AccountTreeIcon from "@mui/icons-material/AccountTree";
import GridViewIcon from "@mui/icons-material/GridView";
const reports = {
  id: "reports",
  title: "Reportes Web",
  type: "group",
  children: [
    {
      id: "page-cpe",
      title: "Reporte CPE",
      type: "item",
      url: "/reportes/reporte-cpe",
      icon: CalendarTodayIcon,
      breadcrumbs: false,
      disabled: false,
    },
    {
      id: "page-posicion",
      title: "Reporte Posición",
      type: "item",
      url: "https://roswebp.acacoop.com.ar/SILReportes/",
      icon: AccountTreeIcon,
      breadcrumbs: false,
      disabled: false,
      external: true,
    },
    {
      id: "page-newReport",
      title: "Panel de Control Logistico",
      type: "item",
      url: "/panelControlLogistico",
      icon: GridViewIcon,
      breadcrumbs: false,
      disabled: false,
    },
  ],
};

export default reports;
