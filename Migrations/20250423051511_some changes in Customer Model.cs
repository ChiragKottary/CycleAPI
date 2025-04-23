using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CycleAPI.Migrations
{
    /// <inheritdoc />
    public partial class somechangesinCustomerModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "customers",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "customers");
        }
    }
}
