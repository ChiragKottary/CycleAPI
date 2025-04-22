using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CycleAPI.Migrations
{
    /// <inheritdoc />
    public partial class EnhancedModelsWithAdditionalFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CycleId1",
                table: "stock_movements",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId1",
                table: "orders",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CycleId1",
                table: "order_items",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BrandId1",
                table: "cycles",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WarrantyMonths",
                table: "cycles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "customers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastLoginDate",
                table: "customers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MarketingPreferences",
                table: "customers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreferredLanguage",
                table: "customers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReferralSource",
                table: "customers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CycleId1",
                table: "cart_items",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Subtotal",
                table: "cart_items",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitPrice",
                table: "cart_items",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Brands",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Brands",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_stock_movements_CycleId1",
                table: "stock_movements",
                column: "CycleId1");

            migrationBuilder.CreateIndex(
                name: "IX_orders_CustomerId1",
                table: "orders",
                column: "CustomerId1");

            migrationBuilder.CreateIndex(
                name: "IX_order_items_CycleId1",
                table: "order_items",
                column: "CycleId1");

            migrationBuilder.CreateIndex(
                name: "IX_cycles_BrandId1",
                table: "cycles",
                column: "BrandId1");

            migrationBuilder.CreateIndex(
                name: "IX_cart_items_CycleId1",
                table: "cart_items",
                column: "CycleId1");

            migrationBuilder.AddForeignKey(
                name: "FK_cart_items_cycles_CycleId1",
                table: "cart_items",
                column: "CycleId1",
                principalTable: "cycles",
                principalColumn: "CycleId");

            migrationBuilder.AddForeignKey(
                name: "FK_cycles_Brands_BrandId1",
                table: "cycles",
                column: "BrandId1",
                principalTable: "Brands",
                principalColumn: "BrandId");

            migrationBuilder.AddForeignKey(
                name: "FK_order_items_cycles_CycleId1",
                table: "order_items",
                column: "CycleId1",
                principalTable: "cycles",
                principalColumn: "CycleId");

            migrationBuilder.AddForeignKey(
                name: "FK_orders_customers_CustomerId1",
                table: "orders",
                column: "CustomerId1",
                principalTable: "customers",
                principalColumn: "CustomerId");

            migrationBuilder.AddForeignKey(
                name: "FK_stock_movements_cycles_CycleId1",
                table: "stock_movements",
                column: "CycleId1",
                principalTable: "cycles",
                principalColumn: "CycleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_cart_items_cycles_CycleId1",
                table: "cart_items");

            migrationBuilder.DropForeignKey(
                name: "FK_cycles_Brands_BrandId1",
                table: "cycles");

            migrationBuilder.DropForeignKey(
                name: "FK_order_items_cycles_CycleId1",
                table: "order_items");

            migrationBuilder.DropForeignKey(
                name: "FK_orders_customers_CustomerId1",
                table: "orders");

            migrationBuilder.DropForeignKey(
                name: "FK_stock_movements_cycles_CycleId1",
                table: "stock_movements");

            migrationBuilder.DropIndex(
                name: "IX_stock_movements_CycleId1",
                table: "stock_movements");

            migrationBuilder.DropIndex(
                name: "IX_orders_CustomerId1",
                table: "orders");

            migrationBuilder.DropIndex(
                name: "IX_order_items_CycleId1",
                table: "order_items");

            migrationBuilder.DropIndex(
                name: "IX_cycles_BrandId1",
                table: "cycles");

            migrationBuilder.DropIndex(
                name: "IX_cart_items_CycleId1",
                table: "cart_items");

            migrationBuilder.DropColumn(
                name: "CycleId1",
                table: "stock_movements");

            migrationBuilder.DropColumn(
                name: "CustomerId1",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "CycleId1",
                table: "order_items");

            migrationBuilder.DropColumn(
                name: "BrandId1",
                table: "cycles");

            migrationBuilder.DropColumn(
                name: "WarrantyMonths",
                table: "cycles");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "LastLoginDate",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "MarketingPreferences",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "PreferredLanguage",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "ReferralSource",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "CycleId1",
                table: "cart_items");

            migrationBuilder.DropColumn(
                name: "Subtotal",
                table: "cart_items");

            migrationBuilder.DropColumn(
                name: "UnitPrice",
                table: "cart_items");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Brands");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Brands");
        }
    }
}
