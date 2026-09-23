using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    public partial class RemoveRelationCuposCuentas : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cuenta_Cupo_CupoId",
                table: "Cuenta");

            migrationBuilder.DropIndex(
                name: "IX_Cuenta_CupoId",
                table: "Cuenta");

            migrationBuilder.DropColumn(
                name: "CupoId",
                table: "Cuenta");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "CupoId",
                table: "Cuenta",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cuenta_CupoId",
                table: "Cuenta",
                column: "CupoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cuenta_Cupo_CupoId",
                table: "Cuenta",
                column: "CupoId",
                principalTable: "Cupo",
                principalColumn: "Id");
        }
    }
}
