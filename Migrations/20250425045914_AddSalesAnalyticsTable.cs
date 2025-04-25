using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CycleAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddSalesAnalyticsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "sales_analytics",
                columns: table => new
                {
                    AnalyticsId = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DailyRevenue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    MonthlyRevenue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    YearlyRevenue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalOrders = table.Column<int>(type: "integer", nullable: false),
                    TotalUnitsSold = table.Column<int>(type: "integer", nullable: false),
                    AverageOrderValue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    GrossProfit = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    NetProfit = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ProfitMargin = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    TopSellingCycleId = table.Column<Guid>(type: "uuid", nullable: true),
                    TopSellingBrandId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sales_analytics", x => x.AnalyticsId);
                    table.ForeignKey(
                        name: "FK_sales_analytics_Brands_TopSellingBrandId",
                        column: x => x.TopSellingBrandId,
                        principalTable: "Brands",
                        principalColumn: "BrandId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_sales_analytics_cycles_TopSellingCycleId",
                        column: x => x.TopSellingCycleId,
                        principalTable: "cycles",
                        principalColumn: "CycleId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_sales_analytics_TopSellingBrandId",
                table: "sales_analytics",
                column: "TopSellingBrandId");

            migrationBuilder.CreateIndex(
                name: "IX_sales_analytics_TopSellingCycleId",
                table: "sales_analytics",
                column: "TopSellingCycleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "sales_analytics");
        }
    }
}
