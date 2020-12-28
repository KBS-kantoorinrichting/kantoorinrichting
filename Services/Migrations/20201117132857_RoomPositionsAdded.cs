using Microsoft.EntityFrameworkCore.Migrations;

namespace Services.Migrations {
    public partial class RoomPositionsAdded : Migration {
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropColumn(
                "Length",
                "Rooms"
            );

            migrationBuilder.DropColumn(
                "Width",
                "Rooms"
            );

            migrationBuilder.AddColumn<string>(
                "Positions",
                "Rooms",
                "nvarchar(max)",
                nullable: true
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropColumn(
                "Positions",
                "Rooms"
            );

            migrationBuilder.AddColumn<int>(
                "Length",
                "Rooms",
                "int",
                nullable: false,
                defaultValue: 0
            );

            migrationBuilder.AddColumn<int>(
                "Width",
                "Rooms",
                "int",
                nullable: false,
                defaultValue: 0
            );
        }
    }
}