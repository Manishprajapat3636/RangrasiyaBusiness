using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RangrasiyaBusiness.Migrations
{
    /// <inheritdoc />
    public partial class AddQuotationItemRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_QuotationItems_ItemId",
                table: "QuotationItems",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_QuotationItems_QuotationId",
                table: "QuotationItems",
                column: "QuotationId");

            migrationBuilder.AddForeignKey(
                name: "FK_QuotationItems_Items_ItemId",
                table: "QuotationItems",
                column: "ItemId",
                principalTable: "Items",
                principalColumn: "ItemId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_QuotationItems_Quotations_QuotationId",
                table: "QuotationItems",
                column: "QuotationId",
                principalTable: "Quotations",
                principalColumn: "QuotationId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuotationItems_Items_ItemId",
                table: "QuotationItems");

            migrationBuilder.DropForeignKey(
                name: "FK_QuotationItems_Quotations_QuotationId",
                table: "QuotationItems");

            migrationBuilder.DropIndex(
                name: "IX_QuotationItems_ItemId",
                table: "QuotationItems");

            migrationBuilder.DropIndex(
                name: "IX_QuotationItems_QuotationId",
                table: "QuotationItems");
        }
    }
}
