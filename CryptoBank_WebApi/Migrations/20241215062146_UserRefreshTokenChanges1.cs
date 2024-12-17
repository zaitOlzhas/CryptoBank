using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CryptoBank_WebApi.Migrations
{
    /// <inheritdoc />
    public partial class UserRefreshTokenChanges1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_UserRefreshTokens_Token",
                table: "UserRefreshTokens",
                column: "Token");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserRefreshTokens_Token",
                table: "UserRefreshTokens");
        }
    }
}
