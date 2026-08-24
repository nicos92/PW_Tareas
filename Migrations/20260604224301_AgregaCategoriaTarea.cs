using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TareasBlazor.Migrations
{
    /// <inheritdoc />
    public partial class AgregaCategoriaTarea : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Categoria",
                table: "Tareas",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Categoria",
                table: "Tareas");
        }
    }
}