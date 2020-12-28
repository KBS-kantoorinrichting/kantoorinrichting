using Microsoft.EntityFrameworkCore.Migrations;

namespace Services.Migrations {
    public partial class AddPlexiglass : Migration {
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.AddColumn<string>(
                "Plexiglass",
                "Designs",
                "nvarchar(max)",
                nullable: true
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropColumn(
                "Plexiglass",
                "Designs"
            );
        }
    }
}