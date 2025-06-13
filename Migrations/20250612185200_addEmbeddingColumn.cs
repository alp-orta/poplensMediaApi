using Microsoft.EntityFrameworkCore.Migrations;
using Pgvector;

#nullable disable

namespace poplensMediaApi.Migrations
{
    /// <inheritdoc />
    public partial class addEmbeddingColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.RenameTable(
                name: "Media",
                newName: "Media",
                newSchema: "public");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:vector", ",,");

            migrationBuilder.AddColumn<Vector>(
                name: "Embedding",
                schema: "public",
                table: "Media",
                type: "vector(384)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Embedding",
                schema: "public",
                table: "Media");

            migrationBuilder.RenameTable(
                name: "Media",
                schema: "public",
                newName: "Media");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:vector", ",,");
        }
    }
}
