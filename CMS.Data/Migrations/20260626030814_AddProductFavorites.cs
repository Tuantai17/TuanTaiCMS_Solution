using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMS.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProductFavorites : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductFavorites",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductFavorites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductFavorites_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductFavorites_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductFavorites_CustomerId_ProductId",
                table: "ProductFavorites",
                columns: new[] { "CustomerId", "ProductId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductFavorites_ProductId",
                table: "ProductFavorites",
                column: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductFavorites");
        }
    }
}
