import { useState } from "react";

import MainCard from "components/cards/MainCard";
import InputAutocomplete from "components/input/InputAutocomplete";
import InputSelect from "components/input/InputSelect";
import { InputWithOnlyValidation } from "components/input/InputWithValidation";
import InputDatePicker from "components/input/InputDatePicker";
import { FormLoadingButton } from "components/button";

import { useYupValidationResolver } from "hooks/useYupValidationResolver";
import {
  getAllTiposDeReporte,
  getAllEstadoDeCupoEnStop,
  getCentroAccounts,
  getBuyerAccounts,
  getSellerAccounts,
  getPuertoAccounts,
  getProductos,
} from "services/fetcher/api";

import { AdapterDayjs } from "@mui/x-date-pickers/AdapterDayjs";
import { LocalizationProvider } from "@mui/x-date-pickers/LocalizationProvider";

import { Grid, TextField } from "@mui/material";

import { Controller, useFieldArray, useForm } from "react-hook-form";
import * as Yup from "yup";
import { ReporteCPERequest } from "models/ReporteCPE";

import { getReporteExcelCPE } from "services/fetcher/api";
import Swal from "sweetalert2";

const validations = Yup.object().shape({
  fechaDesde: Yup.date()
    .typeError("Debe ingresar una fecha")
    .required("Requerido"),
  fechaHasta: Yup.date()
    .typeError("Debe ingresar una fecha")
    .required("Requerido"),
});

export default function Summary() {
  const [isLoading, setIsLoading] = useState(false);

  const resolver = useYupValidationResolver(validations);

  const {
    control,
    handleSubmit,
    formState: { errors },
  } = useForm({
    resolver,
    defaultValues: ReporteCPERequest,
  });

  const onHandleSubmit = async (formData) => {
    setIsLoading(true);
    //fetch tabla
    try {
      const response = await getReporteExcelCPE(formData);
      debugger;
      if (!response.success) {
        // Construir mensaje de error con warnings
        let htmlContent = `<p>${response.message || "Error al obtener los datos, consulte a soporte."}</p>`;
        
        // Agregar warnings si existen
        if (response.warnings && response.warnings.length > 0) {
          htmlContent += `
            <hr style="margin: 15px 0;">
            <p style="margin-top: 10px;"><strong>⚠️ Advertencias adicionales:</strong></p>
            <ul style="text-align: left; margin-top: 10px;">
              ${response.warnings.map(w => `<li>${w}</li>`).join('')}
            </ul>
          `;
        }
        Swal.fire({
          title: "Error",
          html: htmlContent,
          icon: "error",
          confirmButtonText: "Aceptar",
          width: '600px'
        });
        return;
      }else {
        // ✅ Éxito sin warnings
        Swal.fire({
          title: "Éxito",
          text: `Archivo descargado correctamente.${response.recordCount ? ` (${response.recordCount} registros)` : ''}`,
          icon: "success",
          timer: 3000,
          showConfirmButton: false,
        });
      }
    } catch (err) {
      debugger;
      Swal.fire({
        title: "Error",
        text: "Error al obtener los datos, consulte a soporte.",
        icon: "error",
        confirmButtonText: "Aceptar",
      });
    } finally {
      setIsLoading(false);
    }
  };

  const { replace: replaceCentros } = useFieldArray({
    control,
    name: "centros",
  });
  const { replace: replaceProductos } = useFieldArray({
    control,
    name: "productos",
  });
  const { replace: replaceVendedores } = useFieldArray({
    control,
    name: "vendedores",
  });
  const { replace: replaceCompradores } = useFieldArray({
    control,
    name: "compradores",
  });
  const { replace: replaceDestinos } = useFieldArray({
    control,
    name: "destinos",
  });

  const handleOnChangeCentros = (event, value) => {
    replaceCentros(value.map((centro) => centro.obj.centro));
  };

  const handleOnChangeProductos = (event, value) => {
    replaceProductos(value.map((producto) => producto.obj.grano.toString()));
  };

  const handleOnChangeVendedores = (event, value) => {
    replaceVendedores(value.map((vendedor) => vendedor.obj.cuenta.toString()));
  };

  const handleOnChangeCompradores = (event, value) => {
    replaceCompradores(
      value.map((comprador) => comprador.obj.cuenta.toString())
    );
  };

  const handleOnChangeDestinos = (event, value) => {
    replaceDestinos(value.map((destino) => destino.obj.cuenta.toString()));
  };

  const handleOptionsSetterCentros = (results) => {
    if (Array.isArray(results)) {
      return results?.map((result) => {
        return { title: result.nombre, obj: result };
      });
    }
    return [];
  };

  const handleOptionsSetterCompradores = (results) => {
    if (Array.isArray(results)) {
      return results?.map((result) => {
        return { title: result.nombre, obj: result };
      });
    }
    return [];
  };

  const handleOptionsSetterVendedores = (results) => {
    if (Array.isArray(results)) {
      return results?.map((result) => {
        return { title: result.nombre, obj: result };
      });
    }
    return [];
  };

  const handleOptionsSetterPuertos = (results) => {
    if (Array.isArray(results)) {
      return results?.map((result) => {
        return { title: result.nombre, obj: result };
      });
    }
    return [];
  };

  const handleOptionsSetterProductos = (results) => {
    if (Array.isArray(results)) {
      return results?.map((result) => {
        return { title: result.nombre, obj: result };
      });
    }
    return [];
  };

  return (
    <MainCard title="Reporte">
      <form onSubmit={handleSubmit(onHandleSubmit)}>
        <Grid container spacing={2}>
          <LocalizationProvider dateAdapter={AdapterDayjs}>
            <Grid item md={3} sm={6} xs={6}>
              <Controller
                name="fechaDesde"
                control={control}
                render={({ field }) => {
                  return (
                    <InputWithOnlyValidation
                      errors={errors}
                      forName="fechaDesde"
                    >
                      <InputDatePicker
                        {...field}
                        renderInput={(params) => {
                          return <TextField {...params} />;
                        }}
                        inputFormat="DD/MM/YYYY"
                        label="Fecha Desde"
                      />
                    </InputWithOnlyValidation>
                  );
                }}
              />
            </Grid>
            <Grid item md={3} sm={6} xs={6}>
              <Controller
                name="fechaHasta"
                control={control}
                render={({ field }) => {
                  return (
                    <InputWithOnlyValidation
                      errors={errors}
                      forName="fechaHasta"
                    >
                      <InputDatePicker
                        {...field}
                        renderInput={(params) => {
                          return <TextField {...params} />;
                        }}
                        inputFormat="DD/MM/YYYY"
                        label="Fecha Hasta"
                      />
                    </InputWithOnlyValidation>
                  );
                }}
              />
            </Grid>
          </LocalizationProvider>
          <Grid item md={3} sm={6} xs={6}>
            <Controller
              name="tipoDeReporte"
              control={control}
              render={({ field }) => {
                return (
                  <InputSelect
                    value={field.value || ""}
                    name={field.name}
                    onChange={field.onChange}
                    onBlur={field.onBlur}
                    inputRef={field.ref}
                    fetcher={getAllTiposDeReporte}
                    optionsSetter={(results) => {
                      if (results) {
                        return Object.keys(results)?.map((key) => {
                          return { label: results[key], value: key };
                        });
                      }
                      return [];
                    }}
                    label={"Tipo de Reporte"}
                  ></InputSelect>
                );
              }}
            />
          </Grid>
          <Grid item md={3} sm={6} xs={6}>
            <Controller
              name="estadoDeCupoEnSTOP"
              control={control}
              render={({ field }) => {
                return (
                  <InputSelect
                    value={field.value || ""}
                    name={field.name}
                    onChange={field.onChange}
                    onBlur={field.onBlur}
                    inputRef={field.ref}
                    fetcher={getAllEstadoDeCupoEnStop}
                    optionsSetter={(results) => {
                      if (results) {
                        return Object.keys(results)?.map((key) => {
                          return { label: results[key], value: key };
                        });
                      }
                      return [];
                    }}
                    label={"Estado de Cupo en STOP"}
                  ></InputSelect>
                );
              }}
            />
          </Grid>
        </Grid>
        <Grid container spacing={2}>
          <Grid item md={6} sm={12} xs={12}>
            <InputAutocomplete
              fetcher={getBuyerAccounts}
              optionsSetter={handleOptionsSetterCompradores}
              placeholder={"Compradores"}
              name="compradores"
              onChange={handleOnChangeCompradores}
            />
          </Grid>
          <Grid item md={6} sm={12} xs={12}>
            <InputAutocomplete
              fetcher={getSellerAccounts}
              optionsSetter={handleOptionsSetterVendedores}
              placeholder={"Vendedores"}
              name="vendedores"
              onChange={handleOnChangeVendedores}
            />
          </Grid>
        </Grid>
        <Grid container spacing={2}>
          <Grid item md={6} sm={12} xs={12}>
            <InputAutocomplete
              fetcher={getCentroAccounts}
              optionsSetter={handleOptionsSetterCentros}
              placeholder={"Centros"}
              name={"centros"}
              onChange={handleOnChangeCentros}
            />
          </Grid>
          <Grid item md={6} sm={12} xs={12}>
            <InputAutocomplete
              fetcher={getProductos}
              optionsSetter={handleOptionsSetterProductos}
              placeholder={"Productos"}
              name="productos"
              onChange={handleOnChangeProductos}
            />
          </Grid>
        </Grid>
        <Grid container spacing={2}>
          <Grid item md={6} sm={12} xs={12}>
            <InputAutocomplete
              fetcher={getPuertoAccounts}
              optionsSetter={handleOptionsSetterPuertos}
              placeholder={"Destinos"}
              name="destinos"
              onChange={handleOnChangeDestinos}
            />
          </Grid>
        </Grid>
        <Grid container justifyContent="flex-end">
          <Grid item>
            <FormLoadingButton isLoading={isLoading}>
              Exportar Excel
            </FormLoadingButton>
          </Grid>
        </Grid>
      </form>
    </MainCard>
  );
}
