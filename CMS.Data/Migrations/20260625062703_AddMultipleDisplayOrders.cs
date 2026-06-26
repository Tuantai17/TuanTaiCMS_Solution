using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMS.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMultipleDisplayOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DisplayOrder",
                table: "Products",
                newName: "DisplayOrderSale");

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrderBestSelling",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrderNew",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisplayOrderBestSelling",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DisplayOrderNew",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "DisplayOrderSale",
                table: "Products",
                newName: "DisplayOrder");
        }
    }
}
