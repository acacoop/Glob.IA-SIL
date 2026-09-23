using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Centro",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Centro", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cupo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Alfanumerico = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CentroCupo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CentroDist = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CodGrano = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NomGrano = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EstadoSTOP = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CTG = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CodDestino = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NomDestino = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CodDestinatario = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NomDestinatario = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CodTitularDeCCPP = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NomTitularDeCCPP = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NomRteComProductor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CodRteComProductor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CodRteComVtaPrimaria = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NomRteComVtaPrimaria = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CodRteComVtaSecundaria = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NomRteComVtaSecundaria = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CodRteComVtaSecundaria2 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NomRteComVtaSecundaria2 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CodMercATermino = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NomMercATermino = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CodCorVtaPrimaria = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NomCorVtaPrimaria = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CodCorVtaSecundaria = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NomCorVtaSecundaria = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CCPP = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EstaSIL = table.Column<bool>(type: "bit", nullable: false),
                    EstadoSIL = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaInformadoSIL = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cupo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ListaContactos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListaContactos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Reporte",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Query = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reporte", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Rol",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rol", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TipoContacto",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipoContacto", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ZonaComercial",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZonaComercial", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cuenta",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NroCuenta = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Cuit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CupoId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cuenta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cuenta_Cupo_CupoId",
                        column: x => x.CupoId,
                        principalTable: "Cupo",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ListaContactosListaContactos",
                columns: table => new
                {
                    ListasHijasId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ListasPadresId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListaContactosListaContactos", x => new { x.ListasHijasId, x.ListasPadresId });
                    table.ForeignKey(
                        name: "FK_ListaContactosListaContactos_ListaContactos_ListasHijasId",
                        column: x => x.ListasHijasId,
                        principalTable: "ListaContactos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ListaContactosListaContactos_ListaContactos_ListasPadresId",
                        column: x => x.ListasPadresId,
                        principalTable: "ListaContactos",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "InstanciaReporte",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReporteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstanciaReporte", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InstanciaReporte_Reporte_ReporteId",
                        column: x => x.ReporteId,
                        principalTable: "Reporte",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Persona",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Apellido = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CuentaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RolId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Usuario = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CentroId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ZonaComercialId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Persona", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Persona_Centro_CentroId",
                        column: x => x.CentroId,
                        principalTable: "Centro",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Persona_Cuenta_CuentaId",
                        column: x => x.CuentaId,
                        principalTable: "Cuenta",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Persona_Rol_RolId",
                        column: x => x.RolId,
                        principalTable: "Rol",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Persona_ZonaComercial_ZonaComercialId",
                        column: x => x.ZonaComercialId,
                        principalTable: "ZonaComercial",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Parametro",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Parametros = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InstanciaReporteId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parametro", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Parametro_InstanciaReporte_InstanciaReporteId",
                        column: x => x.InstanciaReporteId,
                        principalTable: "InstanciaReporte",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Contacto",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PersonaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TipoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Dato = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contacto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contacto_Persona_PersonaId",
                        column: x => x.PersonaId,
                        principalTable: "Persona",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Contacto_TipoContacto_TipoId",
                        column: x => x.TipoId,
                        principalTable: "TipoContacto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ListaContactosPersona",
                columns: table => new
                {
                    ListasPadreId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PersonasId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListaContactosPersona", x => new { x.ListasPadreId, x.PersonasId });
                    table.ForeignKey(
                        name: "FK_ListaContactosPersona_ListaContactos_ListasPadreId",
                        column: x => x.ListasPadreId,
                        principalTable: "ListaContactos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ListaContactosPersona_Persona_PersonasId",
                        column: x => x.PersonasId,
                        principalTable: "Persona",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Contacto_PersonaId",
                table: "Contacto",
                column: "PersonaId");

            migrationBuilder.CreateIndex(
                name: "IX_Contacto_TipoId",
                table: "Contacto",
                column: "TipoId");

            migrationBuilder.CreateIndex(
                name: "IX_Cuenta_CupoId",
                table: "Cuenta",
                column: "CupoId");

            migrationBuilder.CreateIndex(
                name: "IX_InstanciaReporte_ReporteId",
                table: "InstanciaReporte",
                column: "ReporteId");

            migrationBuilder.CreateIndex(
                name: "IX_ListaContactosListaContactos_ListasPadresId",
                table: "ListaContactosListaContactos",
                column: "ListasPadresId");

            migrationBuilder.CreateIndex(
                name: "IX_ListaContactosPersona_PersonasId",
                table: "ListaContactosPersona",
                column: "PersonasId");

            migrationBuilder.CreateIndex(
                name: "IX_Parametro_InstanciaReporteId",
                table: "Parametro",
                column: "InstanciaReporteId");

            migrationBuilder.CreateIndex(
                name: "IX_Persona_CentroId",
                table: "Persona",
                column: "CentroId");

            migrationBuilder.CreateIndex(
                name: "IX_Persona_CuentaId",
                table: "Persona",
                column: "CuentaId");

            migrationBuilder.CreateIndex(
                name: "IX_Persona_RolId",
                table: "Persona",
                column: "RolId");

            migrationBuilder.CreateIndex(
                name: "IX_Persona_ZonaComercialId",
                table: "Persona",
                column: "ZonaComercialId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Contacto");

            migrationBuilder.DropTable(
                name: "ListaContactosListaContactos");

            migrationBuilder.DropTable(
                name: "ListaContactosPersona");

            migrationBuilder.DropTable(
                name: "Parametro");

            migrationBuilder.DropTable(
                name: "TipoContacto");

            migrationBuilder.DropTable(
                name: "ListaContactos");

            migrationBuilder.DropTable(
                name: "Persona");

            migrationBuilder.DropTable(
                name: "InstanciaReporte");

            migrationBuilder.DropTable(
                name: "Centro");

            migrationBuilder.DropTable(
                name: "Cuenta");

            migrationBuilder.DropTable(
                name: "Rol");

            migrationBuilder.DropTable(
                name: "ZonaComercial");

            migrationBuilder.DropTable(
                name: "Reporte");

            migrationBuilder.DropTable(
                name: "Cupo");
        }
    }
}
