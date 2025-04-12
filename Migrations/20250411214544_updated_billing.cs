using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vehicle_Rental_Management_System.Migrations
{
    /// <inheritdoc />
    public partial class updated_billing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CleaningFee",
                table: "Billings",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "InsuranceFee",
                table: "Billings",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CleaningFee",
                table: "Billings");

            migrationBuilder.DropColumn(
                name: "InsuranceFee",
                table: "Billings");
        }
    }
}
