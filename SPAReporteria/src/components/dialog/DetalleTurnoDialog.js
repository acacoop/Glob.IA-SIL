// components/DetalleTurnoModal.jsx
import { Modal, Table, Button, Space } from "antd";
import { DownloadOutlined } from "@ant-design/icons";
import Papa from "papaparse";
import { useState, useEffect } from "react";

const DetalleTurnoModal = ({ open, onClose, detalle }) => {
  if (!detalle) return null;

  const { grano, fecha, estado, cantidad, detalleEstado } = detalle;
  const [selectedRowKeys, setSelectedRowKeys] = useState([]);
  const [currentPage, setCurrentPage] = useState(1);
  const [rows, setRows] = useState([]);

  // 🔹 Cada vez que cambia el detalle, reiniciamos estados y cargamos los datos nuevos
  useEffect(() => {
    if (detalle) {
      setSelectedRowKeys([]);
      setCurrentPage(1);
      // Generar una clave única por fila sin pisar el id original
      const rowsWithKey = detalle.detalleEstado.map((row, index) => ({
        ...row,
        uniqueKey: `${row.id ?? "row"}-${index}-${Date.now()}`,
      }));
      setRows(rowsWithKey);
    } else {
      setRows([]);
    }
  }, [detalle]);

  // 🔹 Al cerrar el modal, limpiamos también los estados
  useEffect(() => {
    if (!open) {
      setSelectedRowKeys([]);
      setCurrentPage(1);
      setRows([]);
    }
  }, [open]);

  const columns = [
    {
      title: "Vendedor",
      dataIndex: "nomVendSIL",
      key: "nomVendSIL",
      fixed: "left",
      width: 150,
      sorter: (a, b) => (a.nomVendSIL || "").localeCompare(b.nomVendSIL || ""),
    },
    {
      title: "Comprador",
      dataIndex: "nomCompSIL",
      key: "nomCompSIL",
      fixed: "left",
      width: 150,
      sorter: (a, b) => (a.nomCompSIL || "").localeCompare(b.nomCompSIL || ""),
    },
    {
      title: "Alfanumérico",
      dataIndex: "alfanumerico",
      key: "alfanumerico",
      fixed: "left",
      width: 150,
      sorter: (a, b) =>
        (a.alfanumerico || "").localeCompare(b.alfanumerico || ""),
    },
    {
      title: "Estado STOP",
      dataIndex: "estadoSTOP",
      key: "estadoSTOP",
    },
    {
      title: "Rte. Com Vta. Primaria",
      dataIndex: "nomRteComVtaPrimaria",
      key: "nomRteComVtaPrimaria",
    },
    {
      title: "Rte. Com Vta. Secundaria",
      dataIndex: "nomRteComVtaSecundaria",
      key: "nomRteComVtaSecundaria",
    },
    {
      title: "Rte. Com Vta. Secundaria 2",
      dataIndex: "nomRteComVtaSecundaria2",
      key: "nomRteComVtaSecundaria2",
    },
    {
      title: "Merc. A Término",
      dataIndex: "nomMercATermino",
      key: "nomMercATermino",
    },
    {
      title: "Cor. Vta. Primaria",
      dataIndex: "nomCorVtaPrimaria",
      key: "nomCorVtaPrimaria",
    },
    {
      title: "Cor. Vta. Secundaria",
      dataIndex: "nomCorVtaSecundaria",
      key: "nomCorVtaSecundaria",
    },
    { title: "Destino", dataIndex: "nomDestino", key: "nomDestino" },
    {
      title: "Destinatario",
      dataIndex: "nomDestinatario",
      key: "nomDestinatario",
    },
    {
      title: "Observación",
      dataIndex: "observacionSIL",
      key: "observacionSIL",
    },
  ].map((col) => ({
    ...col,
    ellipsis: true,
    onCell: () => ({
      style: { whiteSpace: "pre-wrap", wordBreak: "break-word" },
    }),
  }));

  const exportToCSV = (onlySelected = false) => {
    // 🔹 Definimos las columnas a exportar con sus claves del dataset
    const columnsMap = [
      { title: "Vendedor", dataIndex: "nomVendSIL" },
      { title: "Comprador", dataIndex: "nomCompSIL" },
      { title: "Alfanumerico", dataIndex: "alfanumerico" },
      { title: "Estado STOP", dataIndex: "estadoSTOP" },
      { title: "Rte. Com Vta. Primaria", dataIndex: "nomRteComVtaPrimaria" },
      {
        title: "Rte. Com Vta. Secundaria",
        dataIndex: "nomRteComVtaSecundaria",
      },
      {
        title: "Rte. Com Vta. Secundaria 2",
        dataIndex: "nomRteComVtaSecundaria2",
      },
      { title: "Merc. A Termino", dataIndex: "nomMercATermino" },
      { title: "Cor. Vta. Primaria", dataIndex: "nomCorVtaPrimaria" },
      { title: "Cor. Vta. Secundaria", dataIndex: "nomCorVtaSecundaria" },
      { title: "Destino", dataIndex: "nomDestino" },
      { title: "Destinatario", dataIndex: "nomDestinatario" },
      { title: "Observacion", dataIndex: "observacionSIL" },
    ];

    const rowsToExport = onlySelected
      ? rows.filter((row) => selectedRowKeys.includes(row.uniqueKey))
      : rows;

    const dataToExport = rowsToExport.map((row) => {
      const newRow = {};
      columnsMap.forEach((col) => {
        newRow[col.title] = row[col.dataIndex] ?? "";
      });
      return newRow;
    });

    const csv = Papa.unparse(dataToExport, { delimiter: ";" });
    const blob = new Blob([csv], { type: "text/csv;charset=utf-8;" });
    const url = URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = `detalle_cupos_${grano}_${fecha}.csv`;
    a.click();
    URL.revokeObjectURL(url);
  };

  const rowSelection = {
    selectedRowKeys,
    onChange: setSelectedRowKeys,
  };

  return (
    <Modal
      open={open}
      onCancel={onClose}
      footer={null}
      width="95vw"
      style={{
        top: "1.25rem",
        maxWidth: "112.5rem",
        paddingBottom: 0,
      }}
      Style={{
        maxHeight: "calc(100vh - 6.25rem)",
        overflow: "hidden",
        padding: "1rem",
      }}
      centered
      zIndex={1500}
      destroyOnHidden // 🔹 Fuerza desmontar el contenido del modal al cerrarse
    >
      {/* Info y botones */}
      <div
        style={{
          marginBottom: "1rem",
          display: "flex",
          justifyContent: "space-between",
          alignItems: "center",
          flexWrap: "wrap",
          gap: "0.75rem",
        }}
      >
        <div>
          <strong>GRANO:</strong> {grano} | <strong>FECHA:</strong> {fecha} |{" "}
          <strong>ESTADO:</strong> {estado} | <strong>TOTAL REGISTROS:</strong>{" "}
          {cantidad}
        </div>
        <Space style={{ marginTop: 20 }}>
          <Button
            type="primary"
            icon={<DownloadOutlined />}
            onClick={() => exportToCSV(false)}
          >
            Exportar todo
          </Button>
          <Button
            disabled={selectedRowKeys.length === 0}
            icon={<DownloadOutlined />}
            onClick={() => exportToCSV(true)}
          >
            Exportar seleccionados
          </Button>
        </Space>
      </div>

      <Table
        rowKey="uniqueKey"
        columns={columns}
        dataSource={rows}
        rowSelection={{
          ...rowSelection,
          // 🔹 Aseguramos que se use también la uniqueKey en la selección
          getCheckboxProps: (record) => ({ key: record.uniqueKey }),
        }}
        pagination={{
          current: currentPage,
          onChange: setCurrentPage,
          defaultPageSize: 100, //cantidad inicial de filas
          showSizeChanger: true, //permite cambiar el tamaño de página
          pageSizeOptions: ["10", "20", "50", "100", `${rows.length}`],
          position: ["bottomCenter"], //posición de la paginación
          locale: { items_per_page: "por página" }, //cambia el texto
        }}
        bordered
        sticky
        scroll={{
          y: "calc(98vh - 17.5rem)",
          x: "max-content",
        }}
      />
    </Modal>
  );
};

export default DetalleTurnoModal;
