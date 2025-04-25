using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FliesProject.Migrations
{
    /// <inheritdoc />
    public partial class AddLessonQuizMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LessonQuizMappings",
                columns: table => new
                {
                    LessonId = table.Column<int>(type: "int", nullable: false),
                    QuizId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LessonQuizMappings", x => new { x.LessonId, x.QuizId });
                    table.ForeignKey(
                        name: "FK_LessonQuizMappings_Lessons_LessonId",
                        column: x => x.LessonId,
                        principalTable: "Lessons",
                        principalColumn: "lesson_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LessonQuizMappings_quizzes_QuizId",
                        column: x => x.QuizId,
                        principalTable: "quizzes",
                        principalColumn: "quiz_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LessonQuizMappings_QuizId",
                table: "LessonQuizMappings",
                column: "QuizId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LessonQuizMappings");
        }
    }
}
