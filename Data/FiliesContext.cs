using System;
using System.Collections.Generic;
using FliesProject.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace FliesProject.Data;

public partial class FiliesContext : DbContext
{
    public FiliesContext()
    {
    }

    public FiliesContext(DbContextOptions<FiliesContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Certificate> Certificates { get; set; }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<CourseTransaction> CourseTransactions { get; set; }

    public virtual DbSet<Enrollement> Enrollements { get; set; }

    public virtual DbSet<Lesson> Lessons { get; set; }

    public virtual DbSet<Quiz> Quizzes { get; set; }

    public virtual DbSet<QuizAnswer> QuizAnswers { get; set; }

    public virtual DbSet<QuizComment> QuizComments { get; set; }

    public virtual DbSet<QuizQuestion> QuizQuestions { get; set; }

    public virtual DbSet<QuizTransaction> QuizTransactions { get; set; }

    public virtual DbSet<QuizWritingSample> QuizWritingSamples { get; set; }

    public virtual DbSet<Section> Sections { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserCourseProgress> UserCourseProgresses { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=LAPTOP-0QQ0S8QL;Database=Filies;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Certificate>(entity =>
        {
            entity.HasKey(e => e.CertificateId).HasName("PK__certific__E2256D31A590BF5D");

            entity.ToTable("certificates");

            entity.HasIndex(e => e.CertificateCode, "UQ__certific__2283DB56680C4652").IsUnique();

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
                .HasConstraintName("FK__certifica__enrol__7A672E12");
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.CourseId).HasName("PK__Courses__8F1EF7AEF665063E");

            entity.HasIndex(e => e.Title, "UQ__Courses__E52A1BB3465A0473").IsUnique();

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
                .HasConstraintName("FK__Courses__created__2F10007B");

            entity.HasMany(d => d.Quizzes).WithMany(p => p.Courses)
                .UsingEntity<Dictionary<string, object>>(
                    "CourseQuiz",
                    r => r.HasOne<Quiz>().WithMany()
                        .HasForeignKey("QuizId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__course_qu__quiz___73BA3083"),
                    l => l.HasOne<Course>().WithMany()
                        .HasForeignKey("CourseId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__course_qu__cours__72C60C4A"),
                    j =>
                    {
                        j.HasKey("CourseId", "QuizId").HasName("PK__course_q__5DC9F290822D7D88");
                        j.ToTable("course_quizzes");
                        j.IndexerProperty<int>("CourseId").HasColumnName("course_id");
                        j.IndexerProperty<int>("QuizId").HasColumnName("quiz_id");
                    });
        });

        modelBuilder.Entity<CourseTransaction>(entity =>
        {
            entity.HasKey(e => e.TransactionId).HasName("PK__course_t__85C600AF9A588E4E");

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
            entity.Property(e => e.TransactionType)
                .HasMaxLength(50)
                .HasColumnName("transaction_type");

            entity.HasOne(d => d.Enrollement).WithMany(p => p.CourseTransactions)
                .HasForeignKey(d => d.EnrollementId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__course_tr__enrol__7F2BE32F");
        });

        modelBuilder.Entity<Enrollement>(entity =>
        {
            entity.HasKey(e => e.EnrollementId).HasName("PK__enrollem__DD53F5C65C591217");

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
                .HasConstraintName("FK__enrolleme__cours__656C112C");

            entity.HasOne(d => d.Mentor).WithMany(p => p.EnrollementMentors)
                .HasForeignKey(d => d.MentorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__enrolleme__mento__6477ECF3");

            entity.HasOne(d => d.Student).WithMany(p => p.EnrollementStudents)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__enrolleme__statu__6383C8BA");
        });

        modelBuilder.Entity<Lesson>(entity =>
        {
            entity.HasKey(e => e.LessonId).HasName("PK__Lessons__6421F7BE3D82A984");

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
                .HasConstraintName("FK__Lessons__section__36B12243");
        });

        modelBuilder.Entity<Quiz>(entity =>
        {
            entity.HasKey(e => e.QuizId).HasName("PK__quizzes__2D7053EC4DF61765");

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
            entity.HasKey(e => e.AnswerId).HasName("PK__quiz_ans__3372431828929A23");

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
                .HasConstraintName("FK__quiz_answ__quest__4E88ABD4");
        });

        modelBuilder.Entity<QuizComment>(entity =>
        {
            entity.HasKey(e => e.CommentId).HasName("PK__quiz_com__E7957687EBCCD996");

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
                .HasConstraintName("FK__quiz_comm__paren__5CD6CB2B");

            entity.HasOne(d => d.Quiz).WithMany(p => p.QuizComments)
                .HasForeignKey(d => d.QuizId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__quiz_comm__quiz___5AEE82B9");

            entity.HasOne(d => d.User).WithMany(p => p.QuizComments)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__quiz_comm__user___5BE2A6F2");
        });

        modelBuilder.Entity<QuizQuestion>(entity =>
        {
            entity.HasKey(e => e.QuestionId).HasName("PK__quiz_que__2EC21549F2CA8ABD");

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
                .HasConstraintName("FK__quiz_ques__quiz___4AB81AF0");
        });

        modelBuilder.Entity<QuizTransaction>(entity =>
        {
            entity.HasKey(e => e.QuiztransactionId).HasName("PK__quiz_tra__6BB96EFAB4B23D22");

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
                .HasConstraintName("FK__quiz_tran__quiz___412EB0B6");

            entity.HasOne(d => d.User).WithMany(p => p.QuizTransactions)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__quiz_tran__user___403A8C7D");
        });

        modelBuilder.Entity<QuizWritingSample>(entity =>
        {
            entity.HasKey(e => e.SampleId).HasName("PK__quiz_wri__84ACF7BA9223C9E4");

            entity.ToTable("quiz_writing_samples");

            entity.Property(e => e.SampleId).HasColumnName("sample_id");
            entity.Property(e => e.QuestionId).HasColumnName("question_id");
            entity.Property(e => e.SampleAnswer).HasColumnName("sample_answer");

            entity.HasOne(d => d.Question).WithMany(p => p.QuizWritingSamples)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__quiz_writ__quest__5165187F");
        });

        modelBuilder.Entity<Section>(entity =>
        {
            entity.HasKey(e => e.SectionId).HasName("PK__Sections__F842676A9F6AE810");

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
                .HasConstraintName("FK__Sections__course__32E0915F");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__B9BE370FA027A31E");

            entity.HasIndex(e => e.PhoneNumber, "UQ__Users__85FB4E38CD92FC1E").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Users__AB6E6164AA7705F0").IsUnique();

            entity.HasIndex(e => e.Username, "UQ__Users__F3DBC5729881F0EA").IsUnique();

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
            entity.HasKey(e => e.ProgressId).HasName("PK__user_cou__49B3D8C1F7704A82");

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
                .HasConstraintName("FK__user_cour__enrol__6C190EBB");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
