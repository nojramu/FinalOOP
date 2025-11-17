using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartQuiz.Migrations
{
    /// <inheritdoc />
    public partial class AddIsPrivateToQuiz : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Quizzes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Quizzes");
        }
    }
}
