using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AbpGuessGame.Migrations
{
    /// <inheritdoc />
    public partial class Remove_Game_RowVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "AppGames");

            migrationBuilder.AlterColumn<string>(
                name: "ConcurrencyStamp",
                table: "AppGames",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(40)",
                oldMaxLength: 40,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ConcurrencyStamp",
                table: "AppGames",
                type: "character varying(40)",
                maxLength: 40,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(40)",
                oldMaxLength: 40);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "AppGames",
                type: "bytea",
                rowVersion: true,
                nullable: true);
        }
    }
}
