using System;
using System.Collections.Generic;
using FliesProject.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace FliesProject.Data;

public partial class FiliesContext : DbContext
{
    public FiliesContext(DbContextOptions<FiliesContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Certificate> Certificates { get; set; }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<CourseTransaction> CourseTransactions { get; set; }

    public virtual DbSet<Enrollement> Enrollements { get; set; }

    public virtual DbSet<Lesson> Lessons { get; set; }

    public virtual DbSet<PaymentOrder> PaymentOrders { get; set; }

    public virtual DbSet<Quiz> Quizzes { get; set; }

    public virtual DbSet<QuizAnswer> QuizAnswers { get; set; }

    public virtual DbSet<QuizComment> QuizComments { get; set; }

    public virtual DbSet<QuizQuestion> QuizQuestions { get; set; }

    public virtual DbSet<QuizTransaction> QuizTransactions { get; set; }

    public virtual DbSet<QuizWritingSample> QuizWritingSamples { get; set; }

    public virtual DbSet<Section> Sections { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserCourseProgress> UserCourseProgresses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Certificate>(entity =>
        {
            entity.HasKey(e => e.CertificateId).HasName("PK__certific__E2256D31BB1634F3");

            entity.ToTable("certificates");

            entity.HasIndex(e => e.CertificateCode, "UQ__certific__2283DB560D2BC11E").IsUnique();

            entity.Property(e => e.CertificateId).HasColumnName("certificate_id");
            entity.Property(e => e.CertificateCode)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("certificate_code");
            entity.Property(e => e.EnrollementId).HasColumnName("enrollement_id");
            entity.Property(e => e.IssuedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("issued_at");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("pending")
                .HasColumnName("status");

            entity.HasOne(d => d.Enrollement).WithMany(p => p.Certificates)
                .HasForeignKey(d => d.EnrollementId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__certifica__enrol__2DE6D218");
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.CourseId).HasName("PK__Courses__8F1EF7AEB5242975");

            entity.HasIndex(e => e.Title, "UQ__Courses__E52A1BB348C30EC8").IsUnique();

            entity.Property(e => e.CourseId).HasColumnName("course_id");
            entity.Property(e => e.CoursesPicture)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("courses_picture");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("price");
            entity.Property(e => e.Timelimit).HasColumnName("timelimit");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("title");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Courses)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Courses__created__656C112C");

            entity.HasMany(d => d.Quizzes).WithMany(p => p.Courses)
                .UsingEntity<Dictionary<string, object>>(
                    "CourseQuiz",
                    r => r.HasOne<Quiz>().WithMany()
                        .HasForeignKey("QuizId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__course_qu__quiz___2739D489"),
                    l => l.HasOne<Course>().WithMany()
                        .HasForeignKey("CourseId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__course_qu__cours__2645B050"),
                    j =>
                    {
                        j.HasKey("CourseId", "QuizId").HasName("PK__course_q__5DC9F29054CB9DE1");
                        j.ToTable("course_quizzes");
                        j.IndexerProperty<int>("CourseId").HasColumnName("course_id");
                        j.IndexerProperty<int>("QuizId").HasColumnName("quiz_id");
                    });
        });

        modelBuilder.Entity<CourseTransaction>(entity =>
        {
            entity.HasKey(e => e.TransactionId).HasName("PK__course_t__85C600AFBD2A2AD8");

            entity.ToTable("course_transactions");

            entity.Property(e => e.TransactionId).HasColumnName("transaction_id");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("amount");
            entity.Property(e => e.EnrollementId).HasColumnName("enrollement_id");
            entity.Property(e => e.TransactionDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("transaction_date");

            entity.HasOne(d => d.Enrollement).WithMany(p => p.CourseTransactions)
                .HasForeignKey(d => d.EnrollementId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__course_tr__enrol__07C12930");
        });

        modelBuilder.Entity<Enrollement>(entity =>
        {
            entity.HasKey(e => e.EnrollementId).HasName("PK__enrollem__DD53F5C6E4B2B671");

            entity.ToTable("enrollements");

            entity.Property(e => e.EnrollementId).HasColumnName("enrollement_id");
            entity.Property(e => e.CourseId).HasColumnName("course_id");
            entity.Property(e => e.LimitedAt)
                .HasDefaultValueSql("(dateadd(week,(10),getdate()))")
                .HasColumnType("datetime")
                .HasColumnName("limited_at");
            entity.Property(e => e.MentorId).HasColumnName("mentor_id");
            entity.Property(e => e.StartedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("started_at");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("active")
                .HasColumnName("status");
            entity.Property(e => e.StudentId).HasColumnName("student_id");

            entity.HasOne(d => d.Course).WithMany(p => p.Enrollements)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__enrolleme__cours__03F0984C");

            entity.HasOne(d => d.Mentor).WithMany(p => p.EnrollementMentors)
                .HasForeignKey(d => d.MentorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__enrolleme__mento__02FC7413");

            entity.HasOne(d => d.Student).WithMany(p => p.EnrollementStudents)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__enrolleme__statu__02084FDA");
        });

        modelBuilder.Entity<Lesson>(entity =>
        {
            entity.HasKey(e => e.LessonId).HasName("PK__Lessons__6421F7BE0E629295");

            entity.Property(e => e.LessonId).HasColumnName("lesson_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Duration).HasColumnName("duration");
            entity.Property(e => e.SectionId).HasColumnName("section_id");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("title");
            entity.Property(e => e.VideoUrl)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("video_url");

            entity.HasOne(d => d.Section).WithMany(p => p.Lessons)
                .HasForeignKey(d => d.SectionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Lessons__section__6D0D32F4");
        });

        modelBuilder.Entity<PaymentOrder>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__PaymentO__C3905BCF396F97E0");

            entity.ToTable("PaymentOrder");

            entity.Property(e => e.OrderId)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TransactionRef)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.User).WithMany(p => p.PaymentOrders)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PaymentOrder_User");
        });

        modelBuilder.Entity<Quiz>(entity =>
        {
            entity.HasKey(e => e.QuizId).HasName("PK__quizzes__2D7053ECD3FCE747");

            entity.ToTable("quizzes");

            entity.Property(e => e.QuizId).HasColumnName("quiz_id");
            entity.Property(e => e.Content)
                .HasColumnType("text")
                .HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.IsFree)
                .HasDefaultValue(false)
                .HasColumnName("is_free");
            entity.Property(e => e.MediaUrl)
                .HasMaxLength(255)
                .HasColumnName("media_url");
            entity.Property(e => e.Price)
                .HasDefaultValue(0.00m)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("price");
            entity.Property(e => e.QuizType)
                .HasMaxLength(20)
                .HasColumnName("quiz_type");
            entity.Property(e => e.TimeLimit).HasColumnName("time_limit");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
        });

        modelBuilder.Entity<QuizAnswer>(entity =>
        {
            entity.HasKey(e => e.AnswerId).HasName("PK__quiz_ans__3372431808201007");

            entity.ToTable("quiz_answers");

            entity.Property(e => e.AnswerId).HasColumnName("answer_id");
            entity.Property(e => e.AnswerText)
                .HasMaxLength(500)
                .HasColumnName("answer_text");
            entity.Property(e => e.IsCorrect).HasColumnName("is_correct");
            entity.Property(e => e.QuestionId).HasColumnName("question_id");

            entity.HasOne(d => d.Question).WithMany(p => p.QuizAnswers)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__quiz_answ__quest__10566F31");
        });

        modelBuilder.Entity<QuizComment>(entity =>
        {
            entity.HasKey(e => e.CommentId).HasName("PK__quiz_com__E795768722DE0317");

            entity.ToTable("quiz_comments");

            entity.Property(e => e.CommentId).HasColumnName("comment_id");
            entity.Property(e => e.CommentText)
                .HasColumnType("text")
                .HasColumnName("comment_text");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.ParentCommentId).HasColumnName("parent_comment_id");
            entity.Property(e => e.QuizId).HasColumnName("quiz_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.ParentComment).WithMany(p => p.InverseParentComment)
                .HasForeignKey(d => d.ParentCommentId)
                .HasConstraintName("FK__quiz_comm__paren__18EBB532");

            entity.HasOne(d => d.Quiz).WithMany(p => p.QuizComments)
                .HasForeignKey(d => d.QuizId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__quiz_comm__quiz___17036CC0");

            entity.HasOne(d => d.User).WithMany(p => p.QuizComments)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__quiz_comm__user___17F790F9");
        });

        modelBuilder.Entity<QuizQuestion>(entity =>
        {
            entity.HasKey(e => e.QuestionId).HasName("PK__quiz_que__2EC2154947D22CF7");

            entity.ToTable("quiz_questions");

            entity.Property(e => e.QuestionId).HasColumnName("question_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.MediaUrl)
                .HasMaxLength(255)
                .HasColumnName("media_url");
            entity.Property(e => e.QuestionText).HasColumnName("question_text");
            entity.Property(e => e.QuestionType)
                .HasMaxLength(20)
                .HasColumnName("question_type");
            entity.Property(e => e.QuizId).HasColumnName("quiz_id");

            entity.HasOne(d => d.Quiz).WithMany(p => p.QuizQuestions)
                .HasForeignKey(d => d.QuizId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__quiz_ques__quiz___0C85DE4D");
        });

        modelBuilder.Entity<QuizTransaction>(entity =>
        {
            entity.HasKey(e => e.QuiztransactionId).HasName("PK__quiz_tra__6BB96EFA20D884AE");

            entity.ToTable("quiz_transactions");

            entity.Property(e => e.QuiztransactionId).HasColumnName("quiztransaction_id");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("amount");
            entity.Property(e => e.QuizId).HasColumnName("quiz_id");
            entity.Property(e => e.TransactionDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("transaction_date");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Quiz).WithMany(p => p.QuizTransactions)
                .HasForeignKey(d => d.QuizId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__quiz_tran__quiz___778AC167");

            entity.HasOne(d => d.User).WithMany(p => p.QuizTransactions)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__quiz_tran__user___76969D2E");
        });

        modelBuilder.Entity<QuizWritingSample>(entity =>
        {
            entity.HasKey(e => e.SampleId).HasName("PK__quiz_wri__84ACF7BAACEA1AA3");

            entity.ToTable("quiz_writing_samples");

            entity.Property(e => e.SampleId).HasColumnName("sample_id");
            entity.Property(e => e.QuestionId).HasColumnName("question_id");
            entity.Property(e => e.SampleAnswer).HasColumnName("sample_answer");

            entity.HasOne(d => d.Question).WithMany(p => p.QuizWritingSamples)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__quiz_writ__quest__1332DBDC");
        });

        modelBuilder.Entity<Section>(entity =>
        {
            entity.HasKey(e => e.SectionId).HasName("PK__Sections__F842676A7187C182");

            entity.Property(e => e.SectionId).HasColumnName("section_id");
            entity.Property(e => e.CourseId).HasColumnName("course_id");
            entity.Property(e => e.CreateAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("create_at");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.Positition).HasColumnName("positition");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("title");

            entity.HasOne(d => d.Course).WithMany(p => p.Sections)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Sections__course__693CA210");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__B9BE370FA2CDD2DB");

            entity.HasIndex(e => e.Email, "UQ__Users__AB6E616420656A26").IsUnique();

            entity.HasIndex(e => e.Username, "UQ__Users__F3DBC572B0DDA807").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Address)
                .HasMaxLength(100)
                .HasColumnName("_address");
            entity.Property(e => e.AvatarUrl)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("avatar_url");
            entity.Property(e => e.Balance)
                .HasDefaultValue(0.00m)
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Birthday)
                .HasColumnType("datetime")
                .HasColumnName("birthday");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.Fullname)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("fullname");
            entity.Property(e => e.Gender)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.LastLogin).HasColumnType("datetime");
            entity.Property(e => e.Passwordhash)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("passwordhash");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Role).HasMaxLength(50);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("active");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Username)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("username");
        });

        modelBuilder.Entity<UserCourseProgress>(entity =>
        {
            entity.HasKey(e => e.ProgressId).HasName("PK__user_cou__49B3D8C14AA1DB13");

            entity.ToTable("user_course_progress");

            entity.Property(e => e.ProgressId).HasColumnName("progress_id");
            entity.Property(e => e.CompletedLessons)
                .HasDefaultValue(0)
                .HasColumnName("completed_lessons");
            entity.Property(e => e.CompletedQuizzes)
                .HasDefaultValue(0)
                .HasColumnName("completed_quizzes");
            entity.Property(e => e.EnrollementId).HasColumnName("enrollement_id");
            entity.Property(e => e.ProgressPercentage)
                .HasDefaultValue(0.00m)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("progress_percentage");
            entity.Property(e => e.TotalLessons).HasColumnName("total_lessons");
            entity.Property(e => e.TotalQuizzes).HasColumnName("total_quizzes");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Enrollement).WithMany(p => p.UserCourseProgresses)
                .HasForeignKey(d => d.EnrollementId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__user_cour__enrol__1F98B2C1");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
