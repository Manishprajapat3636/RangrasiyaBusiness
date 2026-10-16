using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RangrasiyaBusiness.Migrations
{
    /// <inheritdoc />
    public partial class AddBankDetailsToCompanyProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccountNumber",
                table: "CompanyProfiles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BankName",
                table: "CompanyProfiles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Branch",
                table: "CompanyProfiles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IFSCCode",
                table: "CompanyProfiles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TermsAndConditions",
                table: "CompanyProfiles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountNumber",
                table: "CompanyProfiles");

            migrationBuilder.DropColumn(
                name: "BankName",
                table: "CompanyProfiles");

            migrationBuilder.DropColumn(
                name: "Branch",
                table: "CompanyProfiles");

            migrationBuilder.DropColumn(
                name: "IFSCCode",
                table: "CompanyProfiles");

            migrationBuilder.DropColumn(
                name: "TermsAndConditions",
                table: "CompanyProfiles");
        }
    }
}
