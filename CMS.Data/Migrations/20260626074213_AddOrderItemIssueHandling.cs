using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMS.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderItemIssueHandling : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AdjustedQuantity",
                table: "OrderDetails",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CustomerConfirmedAt",
                table: "OrderDetails",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerConfirmedBy",
                table: "OrderDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerDecision",
                table: "OrderDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DamagedQuantity",
                table: "OrderDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FulfillableQuantity",
                table: "OrderDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "InternalNote",
                table: "OrderDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IssueReason",
                table: "OrderDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "IssueReportedAt",
                table: "OrderDetails",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IssueReportedBy",
                table: "OrderDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IssueType",
                table: "OrderDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ItemStatus",
                table: "OrderDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MissingQuantity",
                table: "OrderDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OriginalQuantity",
                table: "OrderDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "OrderActivityLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    ActionType = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    OldValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PerformedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderActivityLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderActivityLogs_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderItemIssues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    OrderDetailId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    IssueType = table.Column<int>(type: "int", nullable: false),
                    OrderedQuantity = table.Column<int>(type: "int", nullable: false),
                    FulfillableQuantity = table.Column<int>(type: "int", nullable: false),
                    DamagedQuantity = table.Column<int>(type: "int", nullable: false),
                    MissingQuantity = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    InternalNote = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ReportedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ReportedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CustomerDecision = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CustomerNote = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ResolvedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItemIssues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItemIssues_OrderDetails_OrderDetailId",
                        column: x => x.OrderDetailId,
                        principalTable: "OrderDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderItemIssues_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderItemIssues_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderActivityLogs_OrderId",
                table: "OrderActivityLogs",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemIssues_OrderDetailId",
                table: "OrderItemIssues",
                column: "OrderDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemIssues_OrderId",
                table: "OrderItemIssues",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemIssues_ProductId",
                table: "OrderItemIssues",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemIssues_Status",
                table: "OrderItemIssues",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderActivityLogs");

            migrationBuilder.DropTable(
                name: "OrderItemIssues");

            migrationBuilder.DropColumn(
                name: "AdjustedQuantity",
                table: "OrderDetails");

            migrationBuilder.DropColumn(
                name: "CustomerConfirmedAt",
                table: "OrderDetails");

            migrationBuilder.DropColumn(
                name: "CustomerConfirmedBy",
                table: "OrderDetails");

            migrationBuilder.DropColumn(
                name: "CustomerDecision",
                table: "OrderDetails");

            migrationBuilder.DropColumn(
                name: "DamagedQuantity",
                table: "OrderDetails");

            migrationBuilder.DropColumn(
                name: "FulfillableQuantity",
                table: "OrderDetails");

            migrationBuilder.DropColumn(
                name: "InternalNote",
                table: "OrderDetails");

            migrationBuilder.DropColumn(
                name: "IssueReason",
                table: "OrderDetails");

            migrationBuilder.DropColumn(
                name: "IssueReportedAt",
                table: "OrderDetails");

            migrationBuilder.DropColumn(
                name: "IssueReportedBy",
                table: "OrderDetails");

            migrationBuilder.DropColumn(
                name: "IssueType",
                table: "OrderDetails");

            migrationBuilder.DropColumn(
                name: "ItemStatus",
                table: "OrderDetails");

            migrationBuilder.DropColumn(
                name: "MissingQuantity",
                table: "OrderDetails");

            migrationBuilder.DropColumn(
                name: "OriginalQuantity",
                table: "OrderDetails");
        }
    }
}
