using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MisFinanzas.Migrations
{
    /// <inheritdoc />
    public partial class AddLoanCurrentBalance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CurrentBalance",
                table: "Loans",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastAdjustmentDate",
                table: "Loans",
                type: "timestamp without time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentBalance",
                table: "Loans");

            migrationBuilder.DropColumn(
                name: "LastAdjustmentDate",
                table: "Loans");
        }
    }
}
