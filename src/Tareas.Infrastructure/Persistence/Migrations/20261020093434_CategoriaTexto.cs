using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tareas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CategoriaTexto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // SQLite reconstruye la tabla al alterar la columna y copia los valores crudos:
            // hay que convertir los enteros (0..5) a su nombre de enum ANTES del AlterColumn,
            // o el materializado posterior fallaría al hacer Enum.Parse sobre "0".
            migrationBuilder.Sql(
                "UPDATE Tareas SET Categoria = CASE Categoria " +
                "WHEN 0 THEN 'Ninguna' " +
                "WHEN 1 THEN 'Database' " +
                "WHEN 2 THEN 'Backend' " +
                "WHEN 3 THEN 'Frontend' " +
                "WHEN 4 THEN 'DevOps' " +
                "WHEN 5 THEN 'Testing' " +
                "ELSE 'Ninguna' END;");

            migrationBuilder.AlterColumn<string>(
                name: "Categoria",
                table: "Tareas",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Categoria",
                table: "Tareas",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.Sql(
                "UPDATE Tareas SET Categoria = CASE Categoria " +
                "WHEN 'Ninguna' THEN 0 " +
                "WHEN 'Database' THEN 1 " +
                "WHEN 'Backend' THEN 2 " +
                "WHEN 'Frontend' THEN 3 " +
                "WHEN 'DevOps' THEN 4 " +
                "WHEN 'Testing' THEN 5 " +
                "ELSE 0 END;");
        }
    }
}
