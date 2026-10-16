using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RangrasiyaBusiness.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkDetailsToInvoice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PlaceOfWork",
                table: "Invoices",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SACHSNCode",
                table: "Invoices",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WorkOrderNo",
                table: "Invoices",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlaceOfWork",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "SACHSNCode",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "WorkOrderNo",
                table: "Invoices");
        }
    }
}
