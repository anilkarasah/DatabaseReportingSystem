using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DatabaseReportingSystem.Migrations
{
    /// <inheritdoc />
    public partial class ReducePasswordHashSize : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "User",
                type: "character varying(97)",
                maxLength: 97,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(161)",
                oldMaxLength: 161);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "User",
                type: "character varying(161)",
                maxLength: 161,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(97)",
                oldMaxLength: 97);
        }
    }
}
