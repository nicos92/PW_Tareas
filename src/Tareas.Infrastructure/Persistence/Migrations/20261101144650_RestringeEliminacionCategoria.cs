using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tareas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RestringeEliminacionCategoria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tareas_Categorias_CategoryId",
                table: "Tareas");

            migrationBuilder.AddForeignKey(
                name: "FK_Tareas_Categorias_CategoryId",
                table: "Tareas",
                column: "CategoryId",
                principalTable: "Categorias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tareas_Categorias_CategoryId",
                table: "Tareas");

            migrationBuilder.AddForeignKey(
                name: "FK_Tareas_Categorias_CategoryId",
                table: "Tareas",
                column: "CategoryId",
                principalTable: "Categorias",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
