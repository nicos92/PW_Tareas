using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tareas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregaCategorias : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categorias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categorias", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categorias_Name",
                table: "Categorias",
                column: "Name",
                unique: true);

            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "Tareas",
                type: "INTEGER",
                nullable: true);

            // Conserva las categorías del enum anterior: crea una fila por cada valor
            // utilizado y vincula las tareas existentes antes de eliminar la columna vieja.
            migrationBuilder.Sql(
                """
                INSERT OR IGNORE INTO "Categorias" ("Name", "Description")
                SELECT DISTINCT "Categoria", 'Categoría migrada del catálogo anterior.'
                FROM "Tareas"
                WHERE "Categoria" IS NOT NULL
                  AND "Categoria" <> ''
                  AND "Categoria" <> 'Ninguna';
                """);

            migrationBuilder.Sql(
                """
                UPDATE "Tareas"
                SET "CategoryId" = (
                    SELECT "Id" FROM "Categorias" WHERE "Name" = "Tareas"."Categoria"
                )
                WHERE "Categoria" IS NOT NULL
                  AND "Categoria" <> ''
                  AND "Categoria" <> 'Ninguna';
                """);

            migrationBuilder.DropColumn(
                name: "Categoria",
                table: "Tareas");

            migrationBuilder.CreateIndex(
                name: "IX_Tareas_CategoryId",
                table: "Tareas",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tareas_Categorias_CategoryId",
                table: "Tareas",
                column: "CategoryId",
                principalTable: "Categorias",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tareas_Categorias_CategoryId",
                table: "Tareas");

            migrationBuilder.DropIndex(
                name: "IX_Tareas_CategoryId",
                table: "Tareas");

            migrationBuilder.AddColumn<string>(
                name: "Categoria",
                table: "Tareas",
                type: "TEXT",
                nullable: false,
                defaultValue: "Ninguna");

            migrationBuilder.Sql(
                """
                UPDATE "Tareas"
                SET "Categoria" = COALESCE(
                    (SELECT "Name" FROM "Categorias" WHERE "Id" = "Tareas"."CategoryId"),
                    'Ninguna'
                );
                """);

            migrationBuilder.DropTable(
                name: "Categorias");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Tareas");
        }
    }
}
