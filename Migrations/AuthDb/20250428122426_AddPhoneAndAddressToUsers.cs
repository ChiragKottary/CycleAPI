using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CycleAPI.Migrations.AuthDb
{
    /// <inheritdoc />
    public partial class AddPhoneAndAddressToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "users",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "users",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "UserId",
                keyValue: new Guid("61aa79a1-539b-4f68-905b-4856cf0a6bbe"),
                columns: new[] { "Address", "PhoneNumber" },
                values: new object[] { "Admin Address", "1234567890" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "users");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "users");
        }
    }
}
