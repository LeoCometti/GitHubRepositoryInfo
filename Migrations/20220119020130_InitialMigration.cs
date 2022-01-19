using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GitHubRepositoryInfo.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RepositoryInfoItems",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Url = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepositoryInfoItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FileInfoItems",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Extension = table.Column<string>(type: "text", nullable: false),
                    Lines = table.Column<string>(type: "text", nullable: false),
                    Sloc = table.Column<string>(type: "text", nullable: false),
                    Bytes = table.Column<string>(type: "text", nullable: false),
                    RepositoryInfoId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileInfoItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FileInfoItems_RepositoryInfoItems_RepositoryInfoId",
                        column: x => x.RepositoryInfoId,
                        principalTable: "RepositoryInfoItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FileInfoItems_RepositoryInfoId",
                table: "FileInfoItems",
                column: "RepositoryInfoId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FileInfoItems");

            migrationBuilder.DropTable(
                name: "RepositoryInfoItems");
        }
    }
}
