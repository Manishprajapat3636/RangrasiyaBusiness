using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RangrasiyaBusiness.Migrations
{
    public partial class AddItemNameSnapshots : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ItemNameSnapshot",
                table: "InvoiceItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ItemNameSnapshot",
                table: "QuotationItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            // Existing documents get the best available name from the current Items table.
            // New documents will save the name snapshot at creation time.
            migrationBuilder.Sql(@"
                UPDATE ii
                SET ii.ItemNameSnapshot = ISNULL(i.ItemName, '')
                FROM InvoiceItems AS ii
                LEFT JOIN Items AS i ON i.ItemId = ii.ItemId
                WHERE ii.ItemNameSnapshot = '';

                UPDATE qi
                SET qi.ItemNameSnapshot = ISNULL(i.ItemName, '')
                FROM QuotationItems AS qi
                LEFT JOIN Items AS i ON i.ItemId = qi.ItemId
                WHERE qi.ItemNameSnapshot = '';
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "ItemNameSnapshot", table: "InvoiceItems");
            migrationBuilder.DropColumn(name: "ItemNameSnapshot", table: "QuotationItems");
        }
    }
}
