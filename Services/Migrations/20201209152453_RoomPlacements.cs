using Microsoft.EntityFrameworkCore.Migrations;

namespace Services.Migrations {
    public partial class RoomPlacements : Migration {
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.CreateTable(
                "RoomPlacements",
                table => new {
                    RoomPlacementId = table.Column<int>("int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoomId = table.Column<int>("int", nullable: false),
                    Positions = table.Column<int>("nvarchar(max)", nullable: true),
                    Rotation = table.Column<int>("int", nullable: true),
                    Type = table.Column<int>("nvarchar(max)", nullable: true)
                },
                constraints: table => {
                    table.PrimaryKey("PK_RoomPlacements", x => x.RoomPlacementId);
                    table.ForeignKey(
                        "FK_RoomPlacements_Room_RoomId",
                        x => x.RoomId,
                        "Rooms",
                        "RoomId",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropTable(
                "RoomPlacements"
            );
        }
    }
}