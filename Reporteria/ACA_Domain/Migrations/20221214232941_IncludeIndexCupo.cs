using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    public partial class IncludeIndexCupo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "idx_fechaXalfaYdestino",
                table: "Cupo",
                columns: new[] { "Fecha", "Alfanumerico", "CodDestino" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_fechaXalfaYdestino",
                table: "Cupo");
        }
    }
}
