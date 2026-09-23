import { useEffect, useState } from "react";
import { useForm, Controller, useFieldArray } from "react-hook-form";
import {
  Card,
  CardContent,
  CardHeader,
  Divider,
  Grid,
  TextField,
  Accordion,
  AccordionSummary,
  AccordionDetails,
  Typography,
  Button,
} from "@mui/material";
import { LoadingButton } from "@mui/lab";
import ExpandMoreIcon from "@mui/icons-material/ExpandMore";
import RefreshIcon from "@mui/icons-material/Refresh";
import InputDatePicker from "components/input/InputDatePicker";
import InputAutocomplete from "components/input/InputAutocomplete";
import { InputWithOnlyValidation } from "components/input/InputWithValidation";
import { FormLoadingButton } from "components/button";
import { LocalizationProvider } from "@mui/x-date-pickers/LocalizationProvider";
import { AdapterDayjs } from "@mui/x-date-pickers/AdapterDayjs";
import * as Yup from "yup";
import dayjs from "dayjs";
import { useYupValidationResolver } from "hooks/useYupValidationResolver";
import DetalleTurnoDialog from "components/dialog/DetalleTurnoDialog";
import { ReporteCuposRequest } from "models/ReporteCupos/ReporteCuposRequest";
import {
  getProductsByFilter,
  getCentroByFilter,
  getCompradorByFilter,
  getVendedorByFilter,
  getPuertoByFilter,
} from "services/fetcher/api";
import TurnosTable from "./tablaTurnos";
import { postReporteCupos } from "services/fetcher/api/reporteCupos";
import Swal from "sweetalert2";

const validations = Yup.object().shape({
  fecha: Yup.date().typeError("Debe ingresar una fecha").required("Requerido"),
  // productos: Yup.array()
  //   .min(1, "Seleccione al menos un grano")
  //   .required("Requerido"),
});
const REFRESH_INTERVAL = 30; // segundos configurables
const TurnosView = () => {
  //Hooks
  const [detalleSeleccionado, setDetalleSeleccionado] = useState(null);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [datosTabla, setDatosTabla] = useState([]);
  const [isLoadingBuscar, setIsLoadingBuscar] = useState(false);
  const [isLoadingRefresh, setIsLoadingRefresh] = useState(false);
  const [filtrosAplicados, setFiltrosAplicados] = useState(null);
  const [countdown, setCountdown] = useState(REFRESH_INTERVAL); // segundos

  const resolver = useYupValidationResolver(validations);
  const {
    control,
    handleSubmit,
    formState: { errors },
    watch,
    getValues,
  } = useForm({
    resolver,
    defaultValues: ReporteCuposRequest,
  });

  const fetchData = async (payload, setLoader) => {
    setLoader(true);
    try {
      const response = await postReporteCupos(payload);
      if (response.cuadrantes.length > 0) {
        setDatosTabla(response);
        setFiltrosAplicados(payload); // 🔄 guardo filtros usados
        setCountdown(REFRESH_INTERVAL); // 🔄 reinicio contador SIEMPRE después de éxito
      } else {
        setDatosTabla([]);
        setFiltrosAplicados(null); // 🔄 reseteo filtros usados
        setCountdown(REFRESH_INTERVAL); // 🔄 reinicio contador SIEMPRE después de error
        Swal.fire({
          title: "Advertencia",
          text: "No hay datos disponibles para su busqueda.",
          icon: "warning",
          confirmButtonText: "Aceptar",
        });
      }
    } catch (error) {
      Swal.fire({
        title: "Error",
        text: "Error al obtener los datos, consulte a soporte.",
        icon: "error",
        confirmButtonText: "Aceptar",
      });
    } finally {
      setLoader(false);
    }
  };

  // Buscar
  const onHandleSubmit = async (data) => {
    const payload = {
      fecha: dayjs(data.fecha).format("YYYY-MM-DD"),
      compradores: data.compradores,
      vendedores: data.vendedores,
      productos: data.productos,
      destinos: data.destinos,
      centros: data.centros,
    };
    await fetchData(payload, setIsLoadingBuscar);
  };

  // Refrescar (manual o automático)
  const handleRefresh = async () => {
    const filters = getValues(); // tomamos SIEMPRE de los filtros del form
    const payload = {
      fecha: dayjs(filters.fecha).format("YYYY-MM-DD"),
      compradores: filters.compradores,
      vendedores: filters.vendedores,
      productos: filters.productos,
      destinos: filters.destinos,
      centros: filters.centros,
    };
    await fetchData(payload, setIsLoadingRefresh);
  };

  const filtrosHanCambiado = () => {
    if (!filtrosAplicados) return false; // si nunca se buscó, no habilito
    const currentFilters = getValues();
    // Compara cada campo: fecha, compradores, vendedores, productos, destinos, centros
    const keys = [
      "fecha",
      "compradores",
      "vendedores",
      "productos",
      "destinos",
      "centros",
    ];

    return keys.some((key) => {
      const valApplied = filtrosAplicados[key];
      let valCurrent = currentFilters[key];

      // Si es fecha, convierto a YYYY-MM-DD
      if (key === "fecha" && valCurrent) {
        valCurrent = dayjs(valCurrent).format("YYYY-MM-DD");
      }

      // compara arrays o fecha
      if (Array.isArray(valApplied)) {
        if (!Array.isArray(valCurrent)) return true;
        if (valApplied.length !== valCurrent.length) return true;
        for (let i = 0; i < valApplied.length; i++) {
          if (valApplied[i] !== valCurrent[i]) return true;
        }
        return false;
      } else {
        return valApplied !== valCurrent;
      }
    });
  };

  useEffect(() => {
    if (isLoadingRefresh || isLoadingBuscar || !filtrosAplicados) return;

    // si hay cambios en filtros, no contar
    if (filtrosHanCambiado()) return;

    const interval = setInterval(() => {
      setCountdown((prev) => {
        if (prev === 1) {
          handleRefresh(); // refresco automático
          return REFRESH_INTERVAL; // reinicio contador
        }
        return prev - 1;
      });
    }, 1000);

    return () => clearInterval(interval); // limpieza
  }, [
    filtrosAplicados,
    isLoadingRefresh,
    filtrosAplicados,
    filtrosHanCambiado,
  ]);

  const { replace: replaceCompradores } = useFieldArray({
    control,
    name: "compradores",
  });
  const { replace: replaceVendedores } = useFieldArray({
    control,
    name: "vendedores",
  });
  const { replace: replaceDestinos } = useFieldArray({
    control,
    name: "destinos",
  });
  const { replace: replaceCentros } = useFieldArray({
    control,
    name: "centros",
  });
  const { replace: replaceProductos } = useFieldArray({
    control,
    name: "productos",
  });

  const handleOnChangeCentros = (event, value) => {
    replaceCentros(value.map((centro) => String(centro.obj.codigoCentro)));
  };

  const handleOnChangeProductos = (event, value) => {
    replaceProductos(value.map((producto) => String(producto.obj.codigo)));
  };

  const handleOnChangeVendedores = (event, value) => {
    replaceVendedores(value.map((vendedor) => String(vendedor.obj.cuenta)));
  };

  const handleOnChangeCompradores = (event, value) => {
    replaceCompradores(value.map((comprador) => String(comprador.obj.cuenta)));
  };

  const handleOnChangeDestinos = (event, value) => {
    replaceDestinos(value.map((destino) => String(destino.obj.cuenta)));
  };

  const commonSetter = (results, key = "nombre", valueKey = "cuenta") => {
    return Array.isArray(results)
      ? results.map((r) => ({ title: r[key], obj: r }))
      : [];
  };

  //Tomo la fecha seleccionada de mi input datepicker
  const fechaSeleccionada = watch("fecha");

  //Evento click cuando seleccionamos un valor de unas de las celdas de la tabla
  const handleCellClick = (info) => {
    setDetalleSeleccionado(info);
    setDialogOpen(true);
  };

  return (
    <>
      {/* Card del título */}
      <Card elevation={10}>
        <CardContent>
          <Typography
            sx={{
              variant: "subtitle1",
              fontSize: "25px",
              fontWeight: "bold",
              mb: 2,
            }}
          >
            Panel de Control Logistico
          </Typography>
          <Divider />
          <Accordion defaultExpanded={false}>
            <AccordionSummary expandIcon={<ExpandMoreIcon />}>
              <Typography
                variant="subtitle1"
                fontWeight="bold"
                fontSize={"20px"}
              >
                Filtros
              </Typography>
            </AccordionSummary>
            <AccordionDetails>
              <form
                onSubmit={handleSubmit((data) =>
                  onHandleSubmit(data, setIsLoadingBuscar)
                )}
              >
                <LocalizationProvider dateAdapter={AdapterDayjs}>
                  <Grid container spacing={2}>
                    {/* Fecha */}
                    <Grid item md={4} sm={6} xs={6}>
                      <Controller
                        name="fecha"
                        control={control}
                        render={({ field }) => (
                          <InputWithOnlyValidation
                            errors={errors}
                            forName="fecha"
                          >
                            <InputDatePicker
                              {...field}
                              renderInput={(params) => (
                                <TextField {...params} />
                              )}
                              inputFormat="DD/MM/YYYY"
                              label="Fecha"
                            />
                          </InputWithOnlyValidation>
                        )}
                      />
                    </Grid>

                    {/* Grano */}
                    <Grid item md={4} sm={6} xs={6}>
                      <InputWithOnlyValidation
                        forName="productos"
                        errors={errors}
                      >
                        <InputAutocomplete
                          fetcher={getProductsByFilter}
                          optionsSetter={(r) => commonSetter(r)}
                          placeholder="Granos"
                          name="productos"
                          onChange={handleOnChangeProductos}
                        />
                      </InputWithOnlyValidation>
                    </Grid>

                    {/* Centros */}
                    <Grid item md={4} sm={6} xs={6}>
                      <InputAutocomplete
                        fetcher={getCentroByFilter}
                        optionsSetter={(r) => commonSetter(r)}
                        placeholder="Centros"
                        name="centros"
                        onChange={handleOnChangeCentros}
                      />
                    </Grid>

                    {/* Comprador */}
                    <Grid item md={4} sm={6} xs={6}>
                      <InputAutocomplete
                        fetcher={getCompradorByFilter}
                        optionsSetter={(r) => commonSetter(r)}
                        placeholder="Compradores"
                        name="compradores"
                        onChange={handleOnChangeCompradores}
                      />
                    </Grid>

                    {/* Vendedor */}
                    <Grid item md={4} sm={6} xs={6}>
                      <InputAutocomplete
                        fetcher={getVendedorByFilter}
                        optionsSetter={(r) => commonSetter(r)}
                        placeholder="Vendedores"
                        name="vendedores"
                        onChange={handleOnChangeVendedores}
                      />
                    </Grid>

                    {/* Puerto*/}
                    <Grid item md={4} sm={6} xs={6}>
                      <InputAutocomplete
                        fetcher={getPuertoByFilter}
                        optionsSetter={(r) => commonSetter(r)}
                        placeholder="Destinos"
                        name="destinos"
                        onChange={handleOnChangeDestinos}
                      />
                    </Grid>

                    {/* Botón de acción */}
                    <Grid item xs={12} display="flex" justifyContent="flex-end">
                      <FormLoadingButton isLoading={isLoadingBuscar}>
                        Buscar
                      </FormLoadingButton>
                    </Grid>
                  </Grid>
                </LocalizationProvider>
              </form>
            </AccordionDetails>
          </Accordion>
        </CardContent>
      </Card>

      {/* Tabla  */}
      <Card elevation={10} sx={{ mt: 2 }}>
        <CardHeader
          action={
            <LoadingButton
              variant="outlined"
              startIcon={<RefreshIcon />}
              onClick={handleRefresh}
              loading={isLoadingRefresh} // 👈 activa el spinner
              disabled={
                isLoadingBuscar || !filtrosAplicados || filtrosHanCambiado() // se deshabilita si hay cambios en filtros
              }
            >
              {isLoadingRefresh
                ? "Actualizando..."
                : `Refrescar (${countdown}s)`}
            </LoadingButton>
          }
        />
        <CardContent>
          <TurnosTable
            fechaReferencia={fechaSeleccionada}
            datos={datosTabla}
            onCellClick={handleCellClick}
          />
          <DetalleTurnoDialog
            open={dialogOpen}
            onClose={() => setDialogOpen(false)}
            detalle={detalleSeleccionado}
          />
        </CardContent>
      </Card>
    </>
  );
};

export default TurnosView;
