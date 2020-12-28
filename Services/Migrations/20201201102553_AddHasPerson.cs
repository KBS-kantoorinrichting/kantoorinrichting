using Microsoft.EntityFrameworkCore.Migrations;

namespace Services.Migrations {
    public partial class AddHasPerson : Migration {
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.AddColumn<bool>(
                "HasPerson",
                "Products",
                "bit",
                nullable: false,
                defaultValue: false
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropColumn(
                "HasPerson",
                "Products"
            );
        }
    }
}