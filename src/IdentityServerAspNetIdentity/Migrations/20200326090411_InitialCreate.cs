using Microsoft.EntityFrameworkCore.Migrations;

namespace IdentityServerAspNetIdentity.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExternalAuthenticatingScheme",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchemeName = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalAuthenticatingScheme", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TenantInfo",
                columns: table => new
                {
                    TenantId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DomainName = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantInfo", x => x.TenantId);
                });

            migrationBuilder.CreateTable(
                name: "TenantAuthenticatingScheme",
                columns: table => new
                {
                    TenantId = table.Column<int>(nullable: false),
                    SchemeId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantAuthenticatingScheme", x => new { x.TenantId, x.SchemeId });
                    table.ForeignKey(
                        name: "FK_TenantAuthenticatingScheme_ExternalAuthenticatingScheme_SchemeId",
                        column: x => x.SchemeId,
                        principalTable: "ExternalAuthenticatingScheme",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TenantAuthenticatingScheme_TenantInfo_TenantId",
                        column: x => x.TenantId,
                        principalTable: "TenantInfo",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TenantAuthenticatingScheme_SchemeId",
                table: "TenantAuthenticatingScheme",
                column: "SchemeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TenantAuthenticatingScheme");

            migrationBuilder.DropTable(
                name: "ExternalAuthenticatingScheme");

            migrationBuilder.DropTable(
                name: "TenantInfo");
        }
    }
}
