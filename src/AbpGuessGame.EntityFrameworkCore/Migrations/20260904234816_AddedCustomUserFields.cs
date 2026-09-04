using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AbpGuessGame.Migrations
{
    /// <inheritdoc />
    public partial class AddedCustomUserFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BestGuessCount",
                table: "AbpUsers",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "AbpUsers",
                type: "character varying(13)",
                maxLength: 13,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BestGuessCount",
                table: "AbpUsers");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "AbpUsers");
        }
    }
}
