using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RangrasiyaBusiness.Migrations
{
    /// <inheritdoc />
    public partial class AddQuotationConditions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QuotationConditions",
                columns: table => new
                {
                    QuotationConditionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuotationId = table.Column<int>(type: "int", nullable: false),
                    ConditionText = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuotationConditions", x => x.QuotationConditionId);
                    table.ForeignKey(
                        name: "FK_QuotationConditions_Quotations_QuotationId",
                        column: x => x.QuotationId,
                        principalTable: "Quotations",
                        principalColumn: "QuotationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuotationConditions_QuotationId",
                table: "QuotationConditions",
                column: "QuotationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuotationConditions");
        }
    }
}
