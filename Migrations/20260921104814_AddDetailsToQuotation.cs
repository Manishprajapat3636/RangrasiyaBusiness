using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RangrasiyaBusiness.Migrations
{
    /// <inheritdoc />
    public partial class AddDetailsToQuotation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PlaceOfWork",
                table: "Quotations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "QuotationValidity",
                table: "Quotations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WorkOrderNo",
                table: "Quotations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlaceOfWork",
                table: "Quotations");

            migrationBuilder.DropColumn(
                name: "QuotationValidity",
                table: "Quotations");

            migrationBuilder.DropColumn(
                name: "WorkOrderNo",
                table: "Quotations");
        }
    }
}
