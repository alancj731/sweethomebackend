using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sweetbackend.Migrations
{
    /// <inheritdoc />
    public partial class addStorageFolder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StorageFolder",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StorageFolder",
                table: "Users");
        }
    }
}
