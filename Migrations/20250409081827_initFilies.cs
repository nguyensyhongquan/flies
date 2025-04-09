using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FliesProject.Migrations
{
    /// <inheritdoc />
    public partial class initFilies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "quizzes",
                columns: table => new
                {
                    quiz_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    quiz_type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    content = table.Column<string>(type: "text", nullable: true),
                    media_url = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    time_limit = table.Column<int>(type: "int", nullable: true),
                    is_free = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    price = table.Column<decimal>(type: "decimal(18,2)", nullable: true, defaultValue: 0.00m),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__quizzes__2D7053EC4DF61765", x => x.quiz_id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    email = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    passwordhash = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    fullname = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    avatar_url = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(18,2)", nullable: true, defaultValue: 0.00m),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    LastLogin = table.Column<DateTime>(type: "datetime", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, defaultValue: "active"),
                    Gender = table.Column<string>(type: "char(1)", unicode: false, fixedLength: true, maxLength: 1, nullable: true),
                    username = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    birthday = table.Column<DateTime>(type: "datetime", nullable: true),
                    _address = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PhoneNumber = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Users__B9BE370FA027A31E", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "quiz_questions",
                columns: table => new
                {
                    question_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    quiz_id = table.Column<int>(type: "int", nullable: false),
                    question_text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    question_type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    media_url = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__quiz_que__2EC21549F2CA8ABD", x => x.question_id);
                    table.ForeignKey(
                        name: "FK__quiz_ques__quiz___4AB81AF0",
                        column: x => x.quiz_id,
                        principalTable: "quizzes",
                        principalColumn: "quiz_id");
                });

            migrationBuilder.CreateTable(
                name: "Courses",
                columns: table => new
                {
                    course_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    title = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    courses_picture = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    created_by = table.Column<int>(type: "int", nullable: false),
                    timelimit = table.Column<int>(type: "int", nullable: true),
                    price = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Courses__8F1EF7AEF665063E", x => x.course_id);
                    table.ForeignKey(
                        name: "FK__Courses__created__2F10007B",
                        column: x => x.created_by,
                        principalTable: "Users",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "quiz_comments",
                columns: table => new
                {
                    comment_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    quiz_id = table.Column<int>(type: "int", nullable: false),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    comment_text = table.Column<string>(type: "text", nullable: false),
                    parent_comment_id = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__quiz_com__E7957687EBCCD996", x => x.comment_id);
                    table.ForeignKey(
                        name: "FK__quiz_comm__paren__5CD6CB2B",
                        column: x => x.parent_comment_id,
                        principalTable: "quiz_comments",
                        principalColumn: "comment_id");
                    table.ForeignKey(
                        name: "FK__quiz_comm__quiz___5AEE82B9",
                        column: x => x.quiz_id,
                        principalTable: "quizzes",
                        principalColumn: "quiz_id");
                    table.ForeignKey(
                        name: "FK__quiz_comm__user___5BE2A6F2",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "quiz_transactions",
                columns: table => new
                {
                    quiztransaction_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    quiz_id = table.Column<int>(type: "int", nullable: false),
                    amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    transaction_date = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__quiz_tra__6BB96EFAB4B23D22", x => x.quiztransaction_id);
                    table.ForeignKey(
                        name: "FK__quiz_tran__quiz___412EB0B6",
                        column: x => x.quiz_id,
                        principalTable: "quizzes",
                        principalColumn: "quiz_id");
                    table.ForeignKey(
                        name: "FK__quiz_tran__user___403A8C7D",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "quiz_answers",
                columns: table => new
                {
                    answer_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    question_id = table.Column<int>(type: "int", nullable: false),
                    answer_text = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    is_correct = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__quiz_ans__3372431828929A23", x => x.answer_id);
                    table.ForeignKey(
                        name: "FK__quiz_answ__quest__4E88ABD4",
                        column: x => x.question_id,
                        principalTable: "quiz_questions",
                        principalColumn: "question_id");
                });

            migrationBuilder.CreateTable(
                name: "quiz_writing_samples",
                columns: table => new
                {
                    sample_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    question_id = table.Column<int>(type: "int", nullable: false),
                    sample_answer = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__quiz_wri__84ACF7BA9223C9E4", x => x.sample_id);
                    table.ForeignKey(
                        name: "FK__quiz_writ__quest__5165187F",
                        column: x => x.question_id,
                        principalTable: "quiz_questions",
                        principalColumn: "question_id");
                });

            migrationBuilder.CreateTable(
                name: "course_quizzes",
                columns: table => new
                {
                    course_id = table.Column<int>(type: "int", nullable: false),
                    quiz_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__course_q__5DC9F290822D7D88", x => new { x.course_id, x.quiz_id });
                    table.ForeignKey(
                        name: "FK__course_qu__cours__72C60C4A",
                        column: x => x.course_id,
                        principalTable: "Courses",
                        principalColumn: "course_id");
                    table.ForeignKey(
                        name: "FK__course_qu__quiz___73BA3083",
                        column: x => x.quiz_id,
                        principalTable: "quizzes",
                        principalColumn: "quiz_id");
                });

            migrationBuilder.CreateTable(
                name: "enrollements",
                columns: table => new
                {
                    enrollement_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    course_id = table.Column<int>(type: "int", nullable: false),
                    student_id = table.Column<int>(type: "int", nullable: false),
                    mentor_id = table.Column<int>(type: "int", nullable: false),
                    started_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    limited_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(dateadd(week,(10),getdate()))"),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, defaultValue: "active")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__enrollem__DD53F5C65C591217", x => x.enrollement_id);
                    table.ForeignKey(
                        name: "FK__enrolleme__cours__656C112C",
                        column: x => x.course_id,
                        principalTable: "Courses",
                        principalColumn: "course_id");
                    table.ForeignKey(
                        name: "FK__enrolleme__mento__6477ECF3",
                        column: x => x.mentor_id,
                        principalTable: "Users",
                        principalColumn: "user_id");
                    table.ForeignKey(
                        name: "FK__enrolleme__statu__6383C8BA",
                        column: x => x.student_id,
                        principalTable: "Users",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "Sections",
                columns: table => new
                {
                    section_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    course_id = table.Column<int>(type: "int", nullable: false),
                    title = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    positition = table.Column<int>(type: "int", nullable: true),
                    create_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Sections__F842676A9F6AE810", x => x.section_id);
                    table.ForeignKey(
                        name: "FK__Sections__course__32E0915F",
                        column: x => x.course_id,
                        principalTable: "Courses",
                        principalColumn: "course_id");
                });

            migrationBuilder.CreateTable(
                name: "certificates",
                columns: table => new
                {
                    certificate_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    enrollement_id = table.Column<int>(type: "int", nullable: false),
                    certificate_code = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    issued_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, defaultValue: "pending")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__certific__E2256D31A590BF5D", x => x.certificate_id);
                    table.ForeignKey(
                        name: "FK__certifica__enrol__7A672E12",
                        column: x => x.enrollement_id,
                        principalTable: "enrollements",
                        principalColumn: "enrollement_id");
                });

            migrationBuilder.CreateTable(
                name: "course_transactions",
                columns: table => new
                {
                    transaction_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    enrollement_id = table.Column<int>(type: "int", nullable: false),
                    amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    transaction_date = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    transaction_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__course_t__85C600AF9A588E4E", x => x.transaction_id);
                    table.ForeignKey(
                        name: "FK__course_tr__enrol__7F2BE32F",
                        column: x => x.enrollement_id,
                        principalTable: "enrollements",
                        principalColumn: "enrollement_id");
                });

            migrationBuilder.CreateTable(
                name: "user_course_progress",
                columns: table => new
                {
                    progress_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    enrollement_id = table.Column<int>(type: "int", nullable: false),
                    completed_lessons = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    completed_quizzes = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    total_lessons = table.Column<int>(type: "int", nullable: false),
                    total_quizzes = table.Column<int>(type: "int", nullable: false),
                    progress_percentage = table.Column<decimal>(type: "decimal(5,2)", nullable: true, defaultValue: 0.00m),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__user_cou__49B3D8C1F7704A82", x => x.progress_id);
                    table.ForeignKey(
                        name: "FK__user_cour__enrol__6C190EBB",
                        column: x => x.enrollement_id,
                        principalTable: "enrollements",
                        principalColumn: "enrollement_id");
                });

            migrationBuilder.CreateTable(
                name: "Lessons",
                columns: table => new
                {
                    lesson_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    section_id = table.Column<int>(type: "int", nullable: false),
                    title = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    video_url = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    duration = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Lessons__6421F7BE3D82A984", x => x.lesson_id);
                    table.ForeignKey(
                        name: "FK__Lessons__section__36B12243",
                        column: x => x.section_id,
                        principalTable: "Sections",
                        principalColumn: "section_id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_certificates_enrollement_id",
                table: "certificates",
                column: "enrollement_id");

            migrationBuilder.CreateIndex(
                name: "UQ__certific__2283DB56680C4652",
                table: "certificates",
                column: "certificate_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_course_quizzes_quiz_id",
                table: "course_quizzes",
                column: "quiz_id");

            migrationBuilder.CreateIndex(
                name: "IX_course_transactions_enrollement_id",
                table: "course_transactions",
                column: "enrollement_id");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_created_by",
                table: "Courses",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "UQ__Courses__E52A1BB3465A0473",
                table: "Courses",
                column: "title",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_enrollements_course_id",
                table: "enrollements",
                column: "course_id");

            migrationBuilder.CreateIndex(
                name: "IX_enrollements_mentor_id",
                table: "enrollements",
                column: "mentor_id");

            migrationBuilder.CreateIndex(
                name: "IX_enrollements_student_id",
                table: "enrollements",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "IX_Lessons_section_id",
                table: "Lessons",
                column: "section_id");

            migrationBuilder.CreateIndex(
                name: "IX_quiz_answers_question_id",
                table: "quiz_answers",
                column: "question_id");

            migrationBuilder.CreateIndex(
                name: "IX_quiz_comments_parent_comment_id",
                table: "quiz_comments",
                column: "parent_comment_id");

            migrationBuilder.CreateIndex(
                name: "IX_quiz_comments_quiz_id",
                table: "quiz_comments",
                column: "quiz_id");

            migrationBuilder.CreateIndex(
                name: "IX_quiz_comments_user_id",
                table: "quiz_comments",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_quiz_questions_quiz_id",
                table: "quiz_questions",
                column: "quiz_id");

            migrationBuilder.CreateIndex(
                name: "IX_quiz_transactions_quiz_id",
                table: "quiz_transactions",
                column: "quiz_id");

            migrationBuilder.CreateIndex(
                name: "IX_quiz_transactions_user_id",
                table: "quiz_transactions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_quiz_writing_samples_question_id",
                table: "quiz_writing_samples",
                column: "question_id");

            migrationBuilder.CreateIndex(
                name: "IX_Sections_course_id",
                table: "Sections",
                column: "course_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_course_progress_enrollement_id",
                table: "user_course_progress",
                column: "enrollement_id");

            migrationBuilder.CreateIndex(
                name: "UQ__Users__85FB4E38CD92FC1E",
                table: "Users",
                column: "PhoneNumber",
                unique: true,
                filter: "[PhoneNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UQ__Users__AB6E6164AA7705F0",
                table: "Users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__Users__F3DBC5729881F0EA",
                table: "Users",
                column: "username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "certificates");

            migrationBuilder.DropTable(
                name: "course_quizzes");

            migrationBuilder.DropTable(
                name: "course_transactions");

            migrationBuilder.DropTable(
                name: "Lessons");

            migrationBuilder.DropTable(
                name: "quiz_answers");

            migrationBuilder.DropTable(
                name: "quiz_comments");

            migrationBuilder.DropTable(
                name: "quiz_transactions");

            migrationBuilder.DropTable(
                name: "quiz_writing_samples");

            migrationBuilder.DropTable(
                name: "user_course_progress");

            migrationBuilder.DropTable(
                name: "Sections");

            migrationBuilder.DropTable(
                name: "quiz_questions");

            migrationBuilder.DropTable(
                name: "enrollements");

            migrationBuilder.DropTable(
                name: "quizzes");

            migrationBuilder.DropTable(
                name: "Courses");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
