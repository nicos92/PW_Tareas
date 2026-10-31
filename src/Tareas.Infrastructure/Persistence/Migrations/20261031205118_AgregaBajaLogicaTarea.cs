using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tareas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregaBajaLogicaTarea : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Tareas",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Tareas");
        }
    }
}
