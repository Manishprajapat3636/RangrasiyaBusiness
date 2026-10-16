using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RangrasiyaBusiness.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkListModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Department",
                table: "Works");

            migrationBuilder.DropColumn(
                name: "OfficerContact",
                table: "Works");

            migrationBuilder.DropColumn(
                name: "OfficerName",
                table: "Works");

            migrationBuilder.DropColumn(
                name: "Unit",
                table: "Works");

            migrationBuilder.DropColumn(
                name: "WorkSiteAddress",
                table: "Works");

            migrationBuilder.DropColumn(
                name: "WorkType",
                table: "Works");

            migrationBuilder.CreateTable(
                name: "WorkLists",
                columns: table => new
                {
                    WorkListId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WorkOrderNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    QuotationNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BillNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    OfficerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OfficerContact = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WorkType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FromDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ToDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Department = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WorkSiteAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkLists", x => x.WorkListId);
                    table.ForeignKey(
                        name: "FK_WorkLists_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "CustomerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkListMeasurements",
                columns: table => new
                {
                    WorkListMeasurementId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkListId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Length = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Width = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkListMeasurements", x => x.WorkListMeasurementId);
                    table.ForeignKey(
                        name: "FK_WorkListMeasurements_WorkLists_WorkListId",
                        column: x => x.WorkListId,
                        principalTable: "WorkLists",
                        principalColumn: "WorkListId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkListMeasurements_WorkListId",
                table: "WorkListMeasurements",
                column: "WorkListId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkLists_CustomerId",
                table: "WorkLists",
                column: "CustomerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkListMeasurements");

            migrationBuilder.DropTable(
                name: "WorkLists");

            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "Works",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OfficerContact",
                table: "Works",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OfficerName",
                table: "Works",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "Works",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WorkSiteAddress",
                table: "Works",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WorkType",
                table: "Works",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
