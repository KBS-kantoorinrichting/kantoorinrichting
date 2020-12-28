using Microsoft.EntityFrameworkCore.Migrations;

namespace Services.Migrations {
    public partial class ProductPlacementRotation : Migration {
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.AddColumn<int>(
                "Rotation",
                "ProductPlacements",
                "int",
                nullable: false,
                defaultValue: 0
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropColumn(
                "Rotation",
                "ProductPlacements"
            );
        }
    }
}