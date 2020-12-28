using Microsoft.EntityFrameworkCore.Migrations;

namespace Services.Migrations {
    public partial class InitialCreate : Migration {
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.CreateTable(
                "Products",
                table => new {
                    ProductId = table.Column<int>("int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>("nvarchar(max)", nullable: true),
                    Price = table.Column<double>("float", nullable: true),
                    Photo = table.Column<string>("nvarchar(max)", nullable: true),
                    Width = table.Column<int>("int", nullable: false),
                    Length = table.Column<int>("int", nullable: false)
                },
                constraints: table => {
                    table.PrimaryKey("PK_Products", x => x.ProductId);
                }
            );

            migrationBuilder.CreateTable(
                "Rooms",
                table => new {
                    RoomId = table.Column<int>("int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>("nvarchar(max)", nullable: true),
                    Width = table.Column<int>("int", nullable: false),
                    Length = table.Column<int>("int", nullable: false)
                },
                constraints: table => {
                    table.PrimaryKey("PK_Rooms", x => x.RoomId);
                }
            );

            migrationBuilder.CreateTable(
                "Designs",
                table => new {
                    DesignId = table.Column<int>("int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>("nvarchar(max)", nullable: true),
                    RoomId = table.Column<int>("int", nullable: false)
                },
                constraints: table => {
                    table.PrimaryKey("PK_Designs", x => x.DesignId);
                    table.ForeignKey(
                        "FK_Designs_Rooms_RoomId",
                        x => x.RoomId,
                        "Rooms",
                        "RoomId",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                "ProductPlacements",
                table => new {
                    ProductPlacementId = table.Column<int>("int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    X = table.Column<int>("int", nullable: false),
                    Y = table.Column<int>("int", nullable: false),
                    ProductId = table.Column<int>("int", nullable: false),
                    DesignId = table.Column<int>("int", nullable: false)
                },
                constraints: table => {
                    table.PrimaryKey("PK_ProductPlacements", x => x.ProductPlacementId);
                    table.ForeignKey(
                        "FK_ProductPlacements_Designs_DesignId",
                        x => x.DesignId,
                        "Designs",
                        "DesignId",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        "FK_ProductPlacements_Products_ProductId",
                        x => x.ProductId,
                        "Products",
                        "ProductId",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                "IX_Designs_RoomId",
                "Designs",
                "RoomId"
            );

            migrationBuilder.CreateIndex(
                "IX_ProductPlacements_DesignId",
                "ProductPlacements",
                "DesignId"
            );

            migrationBuilder.CreateIndex(
                "IX_ProductPlacements_ProductId",
                "ProductPlacements",
                "ProductId"
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropTable(
                "ProductPlacements"
            );

            migrationBuilder.DropTable(
                "Designs"
            );

            migrationBuilder.DropTable(
                "Products"
            );

            migrationBuilder.DropTable(
                "Rooms"
            );
        }
    }
}