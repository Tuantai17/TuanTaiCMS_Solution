using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMS.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddParentAndImageToCategoryProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "CategoriesProducts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ParentId",
                table: "CategoriesProducts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CategoriesProducts_ParentId",
                table: "CategoriesProducts",
                column: "ParentId");

            migrationBuilder.AddForeignKey(
                name: "FK_CategoriesProducts_CategoriesProducts_ParentId",
                table: "CategoriesProducts",
                column: "ParentId",
                principalTable: "CategoriesProducts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CategoriesProducts_CategoriesProducts_ParentId",
                table: "CategoriesProducts");

            migrationBuilder.DropIndex(
                name: "IX_CategoriesProducts_ParentId",
                table: "CategoriesProducts");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "CategoriesProducts");

            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "CategoriesProducts");
        }
    }
}
