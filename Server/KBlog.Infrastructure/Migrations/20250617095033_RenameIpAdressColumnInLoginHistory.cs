using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KBlog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameIpAdressColumnInLoginHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IpAdress",
                table: "LoginHistories",
                newName: "IpAddress");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IpAddress",
                table: "LoginHistories",
                newName: "IpAdress");
        }
    }
}
