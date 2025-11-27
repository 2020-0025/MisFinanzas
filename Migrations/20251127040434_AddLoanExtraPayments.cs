using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MisFinanzas.Migrations
{
    /// <inheritdoc />
    public partial class AddLoanExtraPayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRecalculated",
                table: "LoanInstallments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "RecalculatedDate",
                table: "LoanInstallments",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LoanExtraPayments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LoanId = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PaidDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoanExtraPayments", x => x.Id);
                    table.CheckConstraint("CK_LoanExtraPayment_Amount", "\"Amount\" > 0");
                    table.ForeignKey(
                        name: "FK_LoanExtraPayments_Loans_LoanId",
                        column: x => x.LoanId,
                        principalTable: "Loans",
                        principalColumn: "LoanId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LoanExtraPayments_LoanId",
                table: "LoanExtraPayments",
                column: "LoanId");

            migrationBuilder.CreateIndex(
                name: "IX_LoanExtraPayments_LoanId_PaidDate",
                table: "LoanExtraPayments",
                columns: new[] { "LoanId", "PaidDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LoanExtraPayments");

            migrationBuilder.DropColumn(
                name: "IsRecalculated",
                table: "LoanInstallments");

            migrationBuilder.DropColumn(
                name: "RecalculatedDate",
                table: "LoanInstallments");
        }
    }
}
