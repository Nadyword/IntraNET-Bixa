using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IntranetCorp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIsProfileComplete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsProfileComplete",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "OnboardingToken",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OnboardingTokenExpiry",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsProfileComplete",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "OnboardingToken",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "OnboardingTokenExpiry",
                table: "AspNetUsers");
        }
    }
}
