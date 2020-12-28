using Microsoft.EntityFrameworkCore.Migrations;

namespace Services.Migrations {
    public partial class looproute : Migration {
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.AddColumn<string>(
                "Route",
                "Designs",
                "nvarchar(max)",
                nullable: true
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropColumn(
                "Route",
                "Designs"
            );
        }
    }
}