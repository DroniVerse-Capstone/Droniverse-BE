using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace Droniverse.Academy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Course",
                columns: table => new
                {
                    CourseID = table.Column<Guid>(type: "char(36)", nullable: false),
                    CreateBy = table.Column<Guid>(type: "char(36)", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "datetime", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Course", x => x.CourseID);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DroneType",
                columns: table => new
                {
                    DroneTypeID = table.Column<Guid>(type: "char(36)", nullable: false),
                    TypeNameVN = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    TypeNameEN = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    DescriptionVN = table.Column<string>(type: "text", nullable: false),
                    DescriptionEN = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DroneType", x => x.DroneTypeID);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Certificate",
                columns: table => new
                {
                    CertificateID = table.Column<Guid>(type: "char(36)", nullable: false),
                    UpdateBy = table.Column<Guid>(type: "char(36)", nullable: false),
                    CourseVersionID = table.Column<Guid>(type: "char(36)", nullable: false),
                    CourseID = table.Column<Guid>(type: "char(36)", nullable: false),
                    CertificateName = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: false),
                    LogoCertificate = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Signature = table.Column<string>(type: "text", nullable: false),
                    AuthorName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    CreateAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreateBy = table.Column<Guid>(type: "char(36)", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Certificate", x => x.CertificateID);
                    table.ForeignKey(
                        name: "FK_Certificate_Course_CourseID",
                        column: x => x.CourseID,
                        principalTable: "Course",
                        principalColumn: "CourseID",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CourseVersion",
                columns: table => new
                {
                    CourseVersionID = table.Column<Guid>(type: "char(36)", nullable: false),
                    CourseID = table.Column<Guid>(type: "char(36)", nullable: false),
                    UpdateBy = table.Column<Guid>(type: "char(36)", nullable: false),
                    TitleVN = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    TitleEN = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    DescriptionVN = table.Column<string>(type: "text", nullable: false),
                    DescriptionEN = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<ulong>(type: "bit", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: false),
                    Level = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false),
                    EstimatedDuration = table.Column<int>(type: "int", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseVersion", x => x.CourseVersionID);
                    table.CheckConstraint("CK_CourseVersion_Level", "`Level` IN ('EASY', 'MEDIUM', 'HARD')");
                    table.CheckConstraint("CK_CourseVersion_Status", "`Status` IN (0, 1)");
                    table.ForeignKey(
                        name: "FK_CourseVersion_Course_CourseID",
                        column: x => x.CourseID,
                        principalTable: "Course",
                        principalColumn: "CourseID",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Enrollment",
                columns: table => new
                {
                    EnrollmentID = table.Column<Guid>(type: "char(36)", nullable: false),
                    CourseID = table.Column<Guid>(type: "char(36)", nullable: false),
                    UserID = table.Column<Guid>(type: "char(36)", nullable: false),
                    ClubID = table.Column<Guid>(type: "char(36)", nullable: true),
                    EnrollDate = table.Column<DateTime>(type: "datetime", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    LastAccessDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    Progress = table.Column<float>(type: "float", nullable: false),
                    Status = table.Column<sbyte>(type: "tinyint", nullable: false),
                    IsCompleted = table.Column<sbyte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enrollment", x => x.EnrollmentID);
                    table.CheckConstraint("CK_Enrollment_Status", "`Status` IN (0,1,2)");
                    table.ForeignKey(
                        name: "FK_Enrollment_Course_CourseID",
                        column: x => x.CourseID,
                        principalTable: "Course",
                        principalColumn: "CourseID",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Drone",
                columns: table => new
                {
                    DroneID = table.Column<Guid>(type: "char(36)", nullable: false),
                    DroneTypeID = table.Column<Guid>(type: "char(36)", nullable: false),
                    DroneNameVN = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    DroneNameEN = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    Manufacturer = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    DescriptionVN = table.Column<string>(type: "text", nullable: false),
                    DescriptionEN = table.Column<string>(type: "text", nullable: false),
                    Height = table.Column<float>(type: "float", nullable: false),
                    Weight = table.Column<float>(type: "float", nullable: false),
                    Status = table.Column<sbyte>(type: "tinyint", nullable: false),
                    Model3DLink = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Drone", x => x.DroneID);
                    table.CheckConstraint("CK_Drone_Status", "`Status` IN (1,2,3)");
                    table.ForeignKey(
                        name: "FK_Drone_DroneType_DroneTypeID",
                        column: x => x.DroneTypeID,
                        principalTable: "DroneType",
                        principalColumn: "DroneTypeID",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserCertificate",
                columns: table => new
                {
                    CertificateID = table.Column<Guid>(type: "char(36)", nullable: false),
                    UserID = table.Column<Guid>(type: "char(36)", nullable: false),
                    SerialNumber = table.Column<Guid>(type: "char(36)", nullable: false),
                    AchievedDate = table.Column<DateTime>(type: "date", nullable: false),
                    Status = table.Column<ulong>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCertificate", x => new { x.UserID, x.CertificateID });
                    table.CheckConstraint("CK_UserCertificate_Status", "`Status` IN (0, 1)");
                    table.ForeignKey(
                        name: "FK_UserCertificate_Certificate_CertificateID",
                        column: x => x.CertificateID,
                        principalTable: "Certificate",
                        principalColumn: "CertificateID",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Code",
                columns: table => new
                {
                    CodeID = table.Column<Guid>(type: "char(36)", nullable: false),
                    CourseVersionID = table.Column<Guid>(type: "char(36)", nullable: false),
                    ExpireDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    Status = table.Column<sbyte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Code", x => x.CodeID);
                    table.CheckConstraint("CK_Code_Status", "`Status` IN (0, 1)");
                    table.ForeignKey(
                        name: "FK_Code_CourseVersion_CourseVersionID",
                        column: x => x.CourseVersionID,
                        principalTable: "CourseVersion",
                        principalColumn: "CourseVersionID",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CourseVersionCategory",
                columns: table => new
                {
                    CategoryID = table.Column<Guid>(type: "char(36)", nullable: false),
                    CourseVersionID = table.Column<Guid>(type: "char(36)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseVersionCategory", x => new { x.CourseVersionID, x.CategoryID });
                    table.ForeignKey(
                        name: "FK_CourseVersionCategory_CourseVersion_CourseVersionID",
                        column: x => x.CourseVersionID,
                        principalTable: "CourseVersion",
                        principalColumn: "CourseVersionID",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Feedback",
                columns: table => new
                {
                    FeedbackID = table.Column<Guid>(type: "char(36)", nullable: false),
                    CourseVersionID = table.Column<Guid>(type: "char(36)", nullable: false),
                    UserID = table.Column<Guid>(type: "char(36)", nullable: false),
                    Rating = table.Column<sbyte>(type: "tinyint", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Feedback", x => x.FeedbackID);
                    table.CheckConstraint("CK_Feedback_Rating", "`Rating` IN (1, 5)");
                    table.ForeignKey(
                        name: "FK_Feedback_CourseVersion_CourseVersionID",
                        column: x => x.CourseVersionID,
                        principalTable: "CourseVersion",
                        principalColumn: "CourseVersionID",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Module",
                columns: table => new
                {
                    ModuleID = table.Column<Guid>(type: "char(36)", nullable: false),
                    CourseID = table.Column<Guid>(type: "char(36)", nullable: false),
                    TitleVN = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    TitleEN = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    ModuleNumber = table.Column<int>(type: "int", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "datetime", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    UpdateAt = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Module", x => x.ModuleID);
                    table.ForeignKey(
                        name: "FK_Module_CourseVersion_CourseID",
                        column: x => x.CourseID,
                        principalTable: "CourseVersion",
                        principalColumn: "CourseVersionID",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RequiredDrone",
                columns: table => new
                {
                    DroneID = table.Column<Guid>(type: "char(36)", nullable: false),
                    CourseVersionID = table.Column<Guid>(type: "char(36)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequiredDrone", x => new { x.CourseVersionID, x.DroneID });
                    table.ForeignKey(
                        name: "FK_RequiredDrone_CourseVersion_CourseVersionID",
                        column: x => x.CourseVersionID,
                        principalTable: "CourseVersion",
                        principalColumn: "CourseVersionID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RequiredDrone_Drone_DroneID",
                        column: x => x.DroneID,
                        principalTable: "Drone",
                        principalColumn: "DroneID",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CodeUsage",
                columns: table => new
                {
                    CodeID = table.Column<Guid>(type: "char(36)", nullable: false),
                    UserID = table.Column<Guid>(type: "char(36)", nullable: false),
                    UsedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CodeUsage", x => new { x.CodeID, x.UserID });
                    table.ForeignKey(
                        name: "FK_CodeUsage_Code_CodeID",
                        column: x => x.CodeID,
                        principalTable: "Code",
                        principalColumn: "CodeID",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Lesson",
                columns: table => new
                {
                    LessonID = table.Column<Guid>(type: "char(36)", nullable: false),
                    ModuleID = table.Column<Guid>(type: "char(36)", nullable: false),
                    Type = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    ReferenceID = table.Column<Guid>(type: "char(36)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lesson", x => x.LessonID);
                    table.CheckConstraint("CK_Lesson_Type", "`Type` IN ('THEORY', 'QUIZ', 'LAB')");
                    table.ForeignKey(
                        name: "FK_Lesson_Module_ModuleID",
                        column: x => x.ModuleID,
                        principalTable: "Module",
                        principalColumn: "ModuleID",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserModule",
                columns: table => new
                {
                    UserID = table.Column<Guid>(type: "char(36)", nullable: false),
                    ModuleID = table.Column<Guid>(type: "char(36)", nullable: false),
                    EnrollDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    CompleteDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Progress = table.Column<float>(type: "float", nullable: false),
                    IsCompleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserModule", x => new { x.ModuleID, x.UserID });
                    table.CheckConstraint("CK_UserModule_Progress", "`Progress` IN (0, 100)");
                    table.ForeignKey(
                        name: "FK_UserModule_Module_ModuleID",
                        column: x => x.ModuleID,
                        principalTable: "Module",
                        principalColumn: "ModuleID",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Lab",
                columns: table => new
                {
                    LabID = table.Column<Guid>(type: "char(36)", nullable: false),
                    LessonID = table.Column<Guid>(type: "char(36)", nullable: false),
                    Type = table.Column<string>(type: "varchar(50)", nullable: false),
                    NameVN = table.Column<string>(type: "varchar(255)", nullable: false),
                    NameEN = table.Column<string>(type: "varchar(255)", nullable: false),
                    DescriptionVN = table.Column<string>(type: "text", nullable: false),
                    DescriptionEN = table.Column<string>(type: "text", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    CreateBy = table.Column<Guid>(type: "char(36)", nullable: false),
                    UpdateBy = table.Column<Guid>(type: "char(36)", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "datetime", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    UpdateAt = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lab", x => x.LabID);
                    table.CheckConstraint("CK_Lab_Type", "`Type` IN ('LEARNING', 'COMPETITION')");
                    table.ForeignKey(
                        name: "FK_Lab_Lesson_LessonID",
                        column: x => x.LessonID,
                        principalTable: "Lesson",
                        principalColumn: "LessonID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Quiz",
                columns: table => new
                {
                    QuizID = table.Column<Guid>(type: "char(36)", nullable: false),
                    TitleVN = table.Column<string>(type: "varchar(255)", nullable: false),
                    TitleEN = table.Column<string>(type: "varchar(255)", nullable: false),
                    DescriptionVN = table.Column<string>(type: "text", nullable: false),
                    DescriptionEN = table.Column<string>(type: "text", nullable: false),
                    TimeLimit = table.Column<int>(type: "int", nullable: false),
                    TotalScore = table.Column<float>(type: "float", nullable: false),
                    PassScore = table.Column<float>(type: "float", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "datetime", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    UpdateAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    CreateBy = table.Column<Guid>(type: "char(36)", nullable: false),
                    UpdateBy = table.Column<Guid>(type: "char(36)", nullable: false),
                    LessonID = table.Column<Guid>(type: "char(36)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quiz", x => x.QuizID);
                    table.ForeignKey(
                        name: "FK_Quiz_Lesson_LessonID",
                        column: x => x.LessonID,
                        principalTable: "Lesson",
                        principalColumn: "LessonID",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Theory",
                columns: table => new
                {
                    TheoryID = table.Column<Guid>(type: "char(36)", nullable: false),
                    LessonID = table.Column<Guid>(type: "char(36)", nullable: false),
                    ContentVN = table.Column<string>(type: "varchar(255)", nullable: false),
                    ContentEN = table.Column<string>(type: "varchar(255)", nullable: false),
                    CreateBy = table.Column<Guid>(type: "char(36)", nullable: false),
                    UpdateBy = table.Column<Guid>(type: "char(36)", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "datetime", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    UpdateAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    EstimatedTime = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Theory", x => x.TheoryID);
                    table.ForeignKey(
                        name: "FK_Theory_Lesson_LessonID",
                        column: x => x.LessonID,
                        principalTable: "Lesson",
                        principalColumn: "LessonID",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserAttempt",
                columns: table => new
                {
                    AttemptID = table.Column<Guid>(type: "char(36)", nullable: false),
                    UserID = table.Column<Guid>(type: "char(36)", nullable: false),
                    IsCompleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AttemptTime = table.Column<int>(type: "int", nullable: false),
                    LessonID = table.Column<Guid>(type: "char(36)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAttempt", x => x.AttemptID);
                    table.ForeignKey(
                        name: "FK_UserAttempt_Lesson_LessonID",
                        column: x => x.LessonID,
                        principalTable: "Lesson",
                        principalColumn: "LessonID",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Report",
                columns: table => new
                {
                    ReportID = table.Column<Guid>(type: "char(36)", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    ResponseVN = table.Column<string>(type: "text", nullable: false),
                    ResponseEN = table.Column<string>(type: "text", nullable: false),
                    LabID = table.Column<Guid>(type: "char(36)", nullable: false),
                    UserID = table.Column<Guid>(type: "char(36)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Report", x => x.ReportID);
                    table.ForeignKey(
                        name: "FK_Report_Lab_LabID",
                        column: x => x.LabID,
                        principalTable: "Lab",
                        principalColumn: "LabID",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserLab",
                columns: table => new
                {
                    UserLabID = table.Column<Guid>(type: "char(36)", nullable: false),
                    UserID = table.Column<Guid>(type: "char(36)", nullable: false),
                    LabID = table.Column<Guid>(type: "char(36)", nullable: false),
                    Solution = table.Column<string>(type: "text", nullable: false),
                    IsCompleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Time = table.Column<float>(type: "float", nullable: false),
                    NumberOfStep = table.Column<int>(type: "int", nullable: false),
                    Length = table.Column<float>(type: "float", nullable: false),
                    FeedbackVN = table.Column<string>(type: "text", nullable: false),
                    FeedbackEN = table.Column<string>(type: "text", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    Point = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLab", x => x.UserLabID);
                    table.ForeignKey(
                        name: "FK_UserLab_Lab_LabID",
                        column: x => x.LabID,
                        principalTable: "Lab",
                        principalColumn: "LabID",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "QuizQuestion",
                columns: table => new
                {
                    QuestionID = table.Column<Guid>(type: "char(36)", nullable: false),
                    ContentVN = table.Column<string>(type: "varchar(255)", nullable: false),
                    ContentEN = table.Column<string>(type: "varchar(255)", nullable: false),
                    Type = table.Column<string>(type: "varchar(30)", nullable: false),
                    Score = table.Column<float>(type: "float", nullable: false),
                    QuizID = table.Column<Guid>(type: "char(36)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizQuestion", x => x.QuestionID);
                    table.CheckConstraint("CK_Lab_Type1", "`Type` IN ('MULTIPLE_CHOICE', 'TRUE_FALSE')");
                    table.ForeignKey(
                        name: "FK_QuizQuestion_Quiz_QuizID",
                        column: x => x.QuizID,
                        principalTable: "Quiz",
                        principalColumn: "QuizID",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "QuizAnswer",
                columns: table => new
                {
                    AnswerID = table.Column<Guid>(type: "char(36)", nullable: false),
                    ContentVN = table.Column<string>(type: "varchar(255)", nullable: false),
                    ContentEN = table.Column<string>(type: "varchar(255)", nullable: false),
                    IsCorrect = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    QuestionID = table.Column<Guid>(type: "char(36)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizAnswer", x => x.AnswerID);
                    table.ForeignKey(
                        name: "FK_QuizAnswer_QuizQuestion_QuestionID",
                        column: x => x.QuestionID,
                        principalTable: "QuizQuestion",
                        principalColumn: "QuestionID",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Certificate_CourseID",
                table: "Certificate",
                column: "CourseID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Code_CourseVersionID",
                table: "Code",
                column: "CourseVersionID");

            migrationBuilder.CreateIndex(
                name: "IX_CourseVersion_CourseID",
                table: "CourseVersion",
                column: "CourseID");

            migrationBuilder.CreateIndex(
                name: "IX_Drone_DroneTypeID",
                table: "Drone",
                column: "DroneTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollment_CourseID",
                table: "Enrollment",
                column: "CourseID");

            migrationBuilder.CreateIndex(
                name: "IX_Feedback_CourseVersionID",
                table: "Feedback",
                column: "CourseVersionID");

            migrationBuilder.CreateIndex(
                name: "IX_Lab_LessonID",
                table: "Lab",
                column: "LessonID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Lesson_ModuleID",
                table: "Lesson",
                column: "ModuleID");

            migrationBuilder.CreateIndex(
                name: "IX_Module_CourseID",
                table: "Module",
                column: "CourseID");

            migrationBuilder.CreateIndex(
                name: "IX_Quiz_LessonID",
                table: "Quiz",
                column: "LessonID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuizAnswer_QuestionID",
                table: "QuizAnswer",
                column: "QuestionID");

            migrationBuilder.CreateIndex(
                name: "IX_QuizQuestion_QuizID",
                table: "QuizQuestion",
                column: "QuizID");

            migrationBuilder.CreateIndex(
                name: "IX_Report_LabID",
                table: "Report",
                column: "LabID");

            migrationBuilder.CreateIndex(
                name: "IX_RequiredDrone_DroneID",
                table: "RequiredDrone",
                column: "DroneID");

            migrationBuilder.CreateIndex(
                name: "IX_Theory_LessonID",
                table: "Theory",
                column: "LessonID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserAttempt_LessonID",
                table: "UserAttempt",
                column: "LessonID");

            migrationBuilder.CreateIndex(
                name: "IX_UserCertificate_CertificateID",
                table: "UserCertificate",
                column: "CertificateID");

            migrationBuilder.CreateIndex(
                name: "IX_UserLab_LabID",
                table: "UserLab",
                column: "LabID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CodeUsage");

            migrationBuilder.DropTable(
                name: "CourseVersionCategory");

            migrationBuilder.DropTable(
                name: "Enrollment");

            migrationBuilder.DropTable(
                name: "Feedback");

            migrationBuilder.DropTable(
                name: "QuizAnswer");

            migrationBuilder.DropTable(
                name: "Report");

            migrationBuilder.DropTable(
                name: "RequiredDrone");

            migrationBuilder.DropTable(
                name: "Theory");

            migrationBuilder.DropTable(
                name: "UserAttempt");

            migrationBuilder.DropTable(
                name: "UserCertificate");

            migrationBuilder.DropTable(
                name: "UserLab");

            migrationBuilder.DropTable(
                name: "UserModule");

            migrationBuilder.DropTable(
                name: "Code");

            migrationBuilder.DropTable(
                name: "QuizQuestion");

            migrationBuilder.DropTable(
                name: "Drone");

            migrationBuilder.DropTable(
                name: "Certificate");

            migrationBuilder.DropTable(
                name: "Lab");

            migrationBuilder.DropTable(
                name: "Quiz");

            migrationBuilder.DropTable(
                name: "DroneType");

            migrationBuilder.DropTable(
                name: "Lesson");

            migrationBuilder.DropTable(
                name: "Module");

            migrationBuilder.DropTable(
                name: "CourseVersion");

            migrationBuilder.DropTable(
                name: "Course");
        }
    }
}
