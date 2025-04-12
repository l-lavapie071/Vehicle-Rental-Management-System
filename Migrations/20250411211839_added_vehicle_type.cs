using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vehicle_Rental_Management_System.Migrations
{
    /// <inheritdoc />
    public partial class added_vehicle_type : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Vehicles",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "Vehicles");
        }
    }
}
