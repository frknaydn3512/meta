using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AdReport.API.Migrations
{
    /// <inheritdoc />
    public partial class AddMetaAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MetaAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AgencyId = table.Column<int>(type: "integer", nullable: false),
                    ClientId = table.Column<int>(type: "integer", nullable: false),
                    AccountId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EncryptedAccessToken = table.Column<string>(type: "text", nullable: false),
                    AccountName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetaAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MetaAccounts_Agencies_AgencyId",
                        column: x => x.AgencyId,
                        principalTable: "Agencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MetaAccounts_AgencyClients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "AgencyClients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MetaAccounts_AgencyId_AccountId",
                table: "MetaAccounts",
                columns: new[] { "AgencyId", "AccountId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MetaAccounts_ClientId",
                table: "MetaAccounts",
                column: "ClientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MetaAccounts");
        }
    }
}
