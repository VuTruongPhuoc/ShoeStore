using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShoeStore.Migrations
{
    /// <inheritdoc />
    public partial class shoestoreUDaccountv3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Gmail",
                table: "Account",
                newName: "Email");

            migrationBuilder.AlterColumn<bool>(
                name: "Status",
                table: "Roles",
                type: "bit",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "Account",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RandomKey",
                table: "Account",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Status",
                table: "Account",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FullName",
                table: "Account");

            migrationBuilder.DropColumn(
                name: "RandomKey",
                table: "Account");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Account");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "Account",
                newName: "Gmail");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Roles",
                type: "int",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");
        }
    }
}
