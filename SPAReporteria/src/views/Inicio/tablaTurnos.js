// components/TurnosTable.jsx
import { useEffect, useState } from "react";
import { Table, Empty, Typography } from "antd";
import dayjs from "dayjs";

const { Text } = Typography;

// Componente para título con truncamiento (más compacto)
const TituloColumna = ({ texto }) => (
  <div
    style={{
      maxWidth: "3rem", // 🔹 más angosto (antes 6rem)
      overflow: "hidden",
      textOverflow: "ellipsis",
      whiteSpace: "nowrap",
      textAlign: "center",
      display: "inline-block",
      verticalAlign: "middle",
      lineHeight: "1.1rem",
      fontSize: "0.75rem", // 🔹 fuente más pequeña
      fontWeight: 500,
    }}
    title={texto} // tooltip al pasar el mouse
  >
    {texto}
  </div>
);

/**
 * Definición de estados: key => nombre de la propiedad en los datos,
 * label => lo que se muestra en la tabla.
 *
 * Ajustá las "key" si en tu payload de backend los campos se llaman distinto.
 */
const columnasPorEstado = [
  { key: "noSTOPCount", label: "No Turneable" },
  // { key: "sinCTGPendDistCount", label: "Sin CTG Pend Dist" },
  { key: "sinCTGCount", label: "Sin CTG" },
  { key: "activadosCount", label: "Activados" },
  { key: "arribadosCount", label: "Arribados" },
  { key: "descargadosCount", label: "Descargados" },
];

const TurnosTable = ({ fechaReferencia, datos = {}, onCellClick }) => {
  const [fechas, setFechas] = useState([]);
  const [datosTransformados, setDatosTransformados] = useState({});

  // Transformar datos del JSON a la estructura esperada por la tabla
  useEffect(() => {
    // asegurarse de que exista el arreglo
    const cuadrantes = Array.isArray(datos.cuadrantes) ? datos.cuadrantes : [];

    const transformados = {};

    cuadrantes.forEach((item) => {
      const granoName = item.nomGrano ?? "SIN_GRANO";
      const fecha = dayjs(item.fecha).format("DD/MM/YYYY");

      // crea estructuras si no existen
      if (!transformados[granoName]) transformados[granoName] = {};
      if (!transformados[granoName][fecha]) {
        transformados[granoName][fecha] = {};
        // inicializamos todas las keys en 0 para no tener 'undefined'
        columnasPorEstado.forEach(({ key }) => {
          transformados[granoName][fecha][key] = 0;
        });
      }

      // sumamos por cada key definida en columnasPorEstado
      columnasPorEstado.forEach(({ key }) => {
        // si la propiedad existe en item la sumamos, si no, 0
        const valor = Number(item[key]) || 0;
        transformados[granoName][fecha][key] += valor;
      });

      // NOTA: si tenés variantes como 'sinCTGPendDistCount' o campos
      // adicionales, agregálos a columnasPorEstado y aquí se sumarán automáticamente.
    });

    setDatosTransformados(transformados);
  }, [datos.cuadrantes]);

  // Generar lista de fechas basadas en fechaReferencia
  useEffect(() => {
    if (fechaReferencia) {
      const fechaBase = dayjs(fechaReferencia);
      const nuevasFechas = [
        fechaBase.subtract(1, "day").format("DD/MM/YYYY"),
        fechaBase.format("DD/MM/YYYY"),
        fechaBase.add(1, "day").format("DD/MM/YYYY"),
        fechaBase.add(2, "day").format("DD/MM/YYYY"),
        fechaBase.add(3, "day").format("DD/MM/YYYY"),
      ];
      setFechas(nuevasFechas);
    } else {
      setFechas([]);
    }
  }, [fechaReferencia]);

  // Construir columnas estáticas + dinámicas
  const columns = [
    {
      title: "Grano",
      dataIndex: "grano",
      key: "grano",
      fixed: "left",
      width: 180,
      onHeaderCell: () => ({ style: { padding: "0.25rem 0.5rem" } }),
      onCell: () => ({ style: { padding: "0.25rem 0.5rem" } }),
      render: (text) => <strong>{text}</strong>,
    },
  ];

  fechas.forEach((fecha) => {
    const children = columnasPorEstado.map(({ key, label }, idx) => ({
      title: <TituloColumna texto={label} />,
      dataIndex: `${fecha}_${key}`,
      key: `${fecha}_${key}`,
      align: "center",
      width: 60,
      className: idx === columnasPorEstado.length - 1 ? "fecha-fin" : "",
      onCell: (record) => ({
        style: {
          cursor: "pointer",
          transition: "all 0.15s ease-in-out",
          padding: "0.25rem",
        },
        onClick: () => {
          const cantidad = record[`${fecha}_${key}`] ?? 0;
          if (onCellClick && cantidad > 0) {
            // buscamos los registros originales (cuadrantes) para ese grano+fecha
            const registros = (datos.cuadrantes || []).filter(
              (c) =>
                (c.nomGrano ?? "") === record.grano &&
                dayjs(c.fecha).format("DD/MM/YYYY") === fecha
            );

            // dependiendo de la key armamos el detalle correspondiente
            let detalleEstado = [];
            switch (key) {
              case "noSTOPCount":
                detalleEstado = registros.flatMap((r) => r.noSTOPDetail || []);
                break;
              case "sinCTGCount":
                detalleEstado = registros.flatMap((r) => r.sinCTGDetail || []);
                break;
              case "activadosCount":
                detalleEstado = registros.flatMap(
                  (r) => r.activadosDetail || []
                );
                break;
              case "arribadosCount":
                detalleEstado = registros.flatMap(
                  (r) => r.arribadosDetail || []
                );
                break;
              case "descargadosCount":
                detalleEstado = registros.flatMap(
                  (r) => r.descargadosDetail || []
                );
                break;
              default:
                detalleEstado = [];
            }

            if (detalleEstado.length > 0) {
              onCellClick({
                grano: record.grano,
                fecha,
                estado: label,
                detalleEstado,
                cantidad,
              });
            }
          }
        },
      }),
      render: (value) => Number(value) || 0,
    }));

    columns.push({
      title: fecha,
      key: fecha,
      align: "center",
      className: `fecha-col`,
      children,
    });
  });

  // Construir dataSource a partir de datosTransformados
  const granos = Object.keys(datosTransformados);
  const dataSource = granos.map((grano, index) => {
    const row = { key: index, grano };
    fechas.forEach((fecha) => {
      columnasPorEstado.forEach(({ key }) => {
        const cantidad = datosTransformados[grano]?.[fecha]?.[key] ?? 0;
        row[`${fecha}_${key}`] = cantidad;
      });
    });
    return row;
  });

  const hayDatos = granos.length > 0 && fechas.length > 0;

  return (
    <div style={{ maxHeight: "70vh" }}>
      <Table
        columns={columns}
        dataSource={dataSource}
        bordered
        size="small"
        sticky
        scroll={{
          x: "max-content",
          y: "55vh",
        }}
        pagination={false}
        locale={{
          emptyText: (
            <Empty
              description={
                <Text strong style={{ fontSize: "1.25rem" }}>
                  {datos.cuadrantes == null
                    ? "Realice una búsqueda para visualizar cupos"
                    : datos.cuadrantes.length === 0
                    ? "No hay datos disponibles"
                    : "Aquí irían los datos"}
                </Text>
              }
              style={{ margin: "3rem 0" }}
            />
          ),
        }}
        rowClassName={(record, index) =>
          index % 2 === 0 ? "" : "ant-table-row-striped-odd"
        }
        className="turnos-table-striped"
      />
      <style>
        {`
          /* 🔹 Línea vertical negra para separar cada columna de fecha */
          .turnos-table-striped .ant-table-thead > tr > th.fecha-col,
          .turnos-table-striped .ant-table-tbody > tr > td.fecha-col {
            border-right: 2px solid #000 !important;
          }

          /* 🔹 Última columna de fecha también con borde si querés */
          .turnos-table-striped .ant-table-thead > tr > th.fecha-col:last-child,
          .turnos-table-striped .ant-table-tbody > tr > td.fecha-col:last-child {
            border-right: 2px solid #000 !important;
          }
          
          /* 🔹 Línea vertical negra entre grupos de fecha */
          .turnos-table-striped .ant-table-thead > tr > th.fecha-fin,
          .turnos-table-striped .ant-table-tbody > tr > td.fecha-fin {
            border-right: 2px solid #000 !important;
          }
        `}
      </style>
    </div>
  );
};

export default TurnosTable;
