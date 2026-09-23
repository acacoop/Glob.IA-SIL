using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    public partial class NewColumnInCupoTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CCPP",
                table: "Cupo",
                newName: "UsuarioSIL");

            migrationBuilder.AlterColumn<short>(
                name: "EstadoSIL",
                table: "Cupo",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<DateTime>(
                name: "CTGDesde",
                table: "Cupo",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CTGHasta",
                table: "Cupo",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "CantHsSalidaCamion",
                table: "Cupo",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CartaPorte",
                table: "Cupo",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CartaPorteFechaCarga",
                table: "Cupo",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CartaPorteVto",
                table: "Cupo",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CodChofer",
                table: "Cupo",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CodEntregador",
                table: "Cupo",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CodFlete",
                table: "Cupo",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "CodLocalidadDestino",
                table: "Cupo",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CodLocalidadOrigen",
                table: "Cupo",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CodTransportista",
                table: "Cupo",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CodVendSIL",
                table: "Cupo",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "ConsultadoPorAfip",
                table: "Cupo",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Cosecha",
                table: "Cupo",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "CreadoPor",
                table: "Cupo",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "CuitChoferAfip",
                table: "Cupo",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CuitCorredorCAfip",
                table: "Cupo",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CuitCorredorVAfip",
                table: "Cupo",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CuitDestinatarioAfip",
                table: "Cupo",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CuitDestinoAfip",
                table: "Cupo",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CuitEntregadorAfip",
                table: "Cupo",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CuitInterAfip",
                table: "Cupo",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CuitInterFleteAfip",
                table: "Cupo",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CuitMaterminoAfip",
                table: "Cupo",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CuitOrigenAfip",
                table: "Cupo",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CuitRemComercialAfip",
                table: "Cupo",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CuitTransportistaAfip",
                table: "Cupo",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Desvio",
                table: "Cupo",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Dominio",
                table: "Cupo",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Dominio1",
                table: "Cupo",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Dominio2",
                table: "Cupo",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "EsAnulado",
                table: "Cupo",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EsRechazado",
                table: "Cupo",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EstaSTOP",
                table: "Cupo",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Estado",
                table: "Cupo",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaActivado",
                table: "Cupo",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaAnulado",
                table: "Cupo",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaArribado",
                table: "Cupo",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaConfirmado",
                table: "Cupo",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaCreado",
                table: "Cupo",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaDescargado",
                table: "Cupo",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaDesviadoD",
                table: "Cupo",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaDesviadoO",
                table: "Cupo",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaModificado",
                table: "Cupo",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaReactivado",
                table: "Cupo",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaRechazado",
                table: "Cupo",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaRegresado",
                table: "Cupo",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaTomado",
                table: "Cupo",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "IdCupoEstadoStop",
                table: "Cupo",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "IdCupoStop",
                table: "Cupo",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<short>(
                name: "IdEstadoEnPlanta",
                table: "Cupo",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<string>(
                name: "IdTurnoDetalle",
                table: "Cupo",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "KMRecorrer",
                table: "Cupo",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "ModificadoPor",
                table: "Cupo",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "MotivoBajaSIL",
                table: "Cupo",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NomChofer",
                table: "Cupo",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NomEntregador",
                table: "Cupo",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NomFlete",
                table: "Cupo",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NomTransportista",
                table: "Cupo",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NomVendSIL",
                table: "Cupo",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NroContrato",
                table: "Cupo",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "NroEstabOrigen",
                table: "Cupo",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NroPlantaRUCA",
                table: "Cupo",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ObservacionSIL",
                table: "Cupo",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "PesoNetoEstimado",
                table: "Cupo",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PesoOriginal",
                table: "Cupo",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RENSPA",
                table: "Cupo",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UltimaLatitud",
                table: "Cupo",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UltimaLongitud",
                table: "Cupo",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "ValidaKM",
                table: "Cupo",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CTGDesde",
                table: "Cupo");

            migrationBuilder.DropColumn(
                name: "CTGHasta",
                table: "Cupo");

            migrationBuilder.DropColumn(
                name: "CantHsSalidaCamion",
                table: "Cupo");

            migrationBuilder.DropColumn(
                name: "CartaPorte",
                table: "Cupo");

            migrationBuilder.DropColumn(
                name: "CartaPorteFechaCarga",
                table: "Cupo");

            migrationBuilder.DropColumn(
                name: "CartaPorteVto",
                table: "Cupo");

            migrationBuilder.DropColumn(
                name: "CodChofer",
                table: "Cupo");

            migrationBuilder.DropColumn(
                name: "CodEntregador",
                table: "Cupo");

            migrationBuilder.DropColumn(
                name: "CodFlete",
                table: "Cupo");

            migrationBuilder.DropColumn(
                name: "CodLocalidadDestino",
                table: "Cupo");

            migrationBuilder.DropColumn(
                name: "CodLocalidadOrigen",
                table: "Cupo");

            migrationBuilder.DropColumn(
                name: "CodTransportista",
                table: "Cupo");

            migrationBuilder.DropColumn(
                name: "CodVendSIL",
                table: "Cupo");

            migrationBuilder.DropColumn(
                name: "ConsultadoPorAfip",
                table: "Cupo");

            migrationBuilder.DropColumn(
                name: "Cosecha",
                table: "Cupo");

            migrationBuilder.DropColumn(
                name: "CreadoPor",
                table: "Cupo");

            migrationBuilder.DropColumn(
                name: "CuitChoferAfip",
                table: "Cupo");

            migrationBuilder.DropColumn(
                name: "CuitCorredorCAfip",
                table: "Cupo");

            migrationBuilder.DropColumn(
                name: "CuitCorredorVAfip",
                table: "Cupo");

            migrationBuilder.DropColumn(
                name: "CuitDestinatarioAfip",
                table: "Cupo");

            migrationBuilder.DropColumn(
                name: "CuitDestinoAfip",
                table: "Cupo");

            migrationBuilder.DropColumn(
                name: "CuitEntregadorAfip",
                table: "Cupo");

            migrationBuilder.DropColumn(
                name: "CuitInterAfip",
                table: "Cupo");

            migrationBuilder.DropColumn(
                name: "CuitInterFleteAfip",
                table: "Cupo");

            migrationBuilder.DropColumn(
                name: "CuitMaterminoAfip",
                table: "Cupo");

            migrationBuilder.DropColumn(
                name: "CuitOrigenAfip",
                table: "Cupo");

            migrationBuilder.DropColumn(
                name: "CuitRemComercialAfip",
                table: "Cupo");

            migrationBuilder.DropColumn(
                name: "CuitTransportistaAfip",
                table: "Cupo");

            migrationBuilder.DropColumn(
                name: "Desvio",
                table: "Cupo");

            migrationBuilder.DropColumn(
                name: "Dominio",
                table: "Cupo");

            migrationBuilder.DropColumn(
                name: "Dominio1",
                table: "Cupo");

            migrationBuilder.DropColumn(
                name: "Dominio2",
                table: "Cupo");

            migrationBuilder.DropColumn(
                name: "EsAnulado",
                table: "Cupo");

            migrationBuilder.DropColumn(
                name: "EsRechazado",
                table: "Cupo");

            migrationBuilder.DropColumn(
                name: "EstaSTOP",
                table: "Cupo");

            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Cupo");

            migrationBuilder.DropColumn(
                name: "FechaActivado",
                table: "Cupo");

            migrationBuilder.DropColumn(
                name: "FechaAnulado",
                table: "Cupo");

            migrationBuilder.DropColumn(
                name: "FechaArribado",
                table: "Cupo");

            migrationBuilder.DropColumn(
                name: "FechaConfirmado",
                table: "Cupo");

            migrationBuilder.DropColumn(
                name: "FechaCreado",
                table: "Cupo");

            migrationBuilder.DropColumn(
                name: "FechaDescargado",
                table: "Cupo");

            migrationBuilder.DropColumn(
                name: "FechaDesviadoD",
                table: "Cupo");

            migrationBuilder.DropColumn(
                name: "FechaDesviadoO",
                table: "Cupo");

            migrationBuilder.DropColumn(
                name: "FechaModificado",
                table: "Cupo");

            migrationBuilder.DropColumn(
                name: "FechaReactivado",
                table: "Cupo");

            migrationBuilder.DropColumn(
                name: "FechaRechazado",
                table: "Cupo");

            migrationBuilder.DropColumn(
                name: "FechaRegresado",
                table: "Cupo");

            migrationBuilder.DropColumn(
                name: "FechaTomado",
                table: "Cupo");

            migrationBuilder.DropColumn(
                name: "IdCupoEstadoStop",
                table: "Cupo");

            migrationBuilder.DropColumn(
                name: "IdCupoStop",
                table: "Cupo");

            migrationBuilder.DropColumn(
                name: "IdEstadoEnPlanta",
                table: "Cupo");

            migrationBuilder.DropColumn(
                name: "IdTurnoDetalle",
                table: "Cupo");

            migrationBuilder.DropColumn(
                name: "KMRecorrer",
                table: "Cupo");

            migrationBuilder.DropColumn(
                name: "ModificadoPor",
                table: "Cupo");

            migrationBuilder.DropColumn(
                name: "MotivoBajaSIL",
                table: "Cupo");

            migrationBuilder.DropColumn(
                name: "NomChofer",
                table: "Cupo");

            migrationBuilder.DropColumn(
                name: "NomEntregador",
                table: "Cupo");

            migrationBuilder.DropColumn(
                name: "NomFlete",
                table: "Cupo");

            migrationBuilder.DropColumn(
                name: "NomTransportista",
                table: "Cupo");

            migrationBuilder.DropColumn(
                name: "NomVendSIL",
                table: "Cupo");

            migrationBuilder.DropColumn(
                name: "NroContrato",
                table: "Cupo");

            migrationBuilder.DropColumn(
                name: "NroEstabOrigen",
                table: "Cupo");

            migrationBuilder.DropColumn(
                name: "NroPlantaRUCA",
                table: "Cupo");

            migrationBuilder.DropColumn(
                name: "ObservacionSIL",
                table: "Cupo");

            migrationBuilder.DropColumn(
                name: "PesoNetoEstimado",
                table: "Cupo");

            migrationBuilder.DropColumn(
                name: "PesoOriginal",
                table: "Cupo");

            migrationBuilder.DropColumn(
                name: "RENSPA",
                table: "Cupo");

            migrationBuilder.DropColumn(
                name: "UltimaLatitud",
                table: "Cupo");

            migrationBuilder.DropColumn(
                name: "UltimaLongitud",
                table: "Cupo");

            migrationBuilder.DropColumn(
                name: "ValidaKM",
                table: "Cupo");

            migrationBuilder.RenameColumn(
                name: "UsuarioSIL",
                table: "Cupo",
                newName: "CCPP");

            migrationBuilder.AlterColumn<string>(
                name: "EstadoSIL",
                table: "Cupo",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");
        }
    }
}
