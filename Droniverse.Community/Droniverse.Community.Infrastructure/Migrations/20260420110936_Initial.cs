using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace Droniverse.Community.Infrastructure.Migrations
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
                name: "Club",
                columns: table => new
                {
                    ClubID = table.Column<Guid>(type: "char(36)", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "char(36)", nullable: false),
                    ManagerID = table.Column<Guid>(type: "char(36)", nullable: false),
                    DroneID = table.Column<Guid>(type: "char(36)", nullable: false),
                    ClubPolicy = table.Column<string>(type: "text", nullable: false),
                    NameVN = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    NameEN = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "varchar(255)", nullable: false),
                    ClubCode = table.Column<string>(type: "char(6)", nullable: false),
                    Status = table.Column<sbyte>(type: "tinyint", nullable: false),
                    ImageUrl = table.Column<string>(type: "varchar(255)", nullable: true),
                    IsPublic = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    LimitParticipation = table.Column<int>(type: "int", nullable: false),
                    LimitClubManagers = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    SuspendedReason = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Club", x => x.ClubID);
                    table.CheckConstraint("CK_Club_Status", "`Status` IN (0, 1, 2, 3)");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MediaType",
                columns: table => new
                {
                    MediaTypeID = table.Column<Guid>(type: "char(36)", nullable: false),
                    TypeNameVN = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    TypeNameEN = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    DescriptionEN = table.Column<string>(type: "text", nullable: false),
                    DescriptionVN = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaType", x => x.MediaTypeID);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ProductCategory",
                columns: table => new
                {
                    CategoryID = table.Column<Guid>(type: "char(36)", nullable: false),
                    Code = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    CategoryNameVN = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    CategoryNameEN = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    DescriptionVN = table.Column<string>(type: "text", nullable: false),
                    DescriptionEN = table.Column<string>(type: "text", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCategory", x => x.CategoryID);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Wallet",
                columns: table => new
                {
                    WalletID = table.Column<Guid>(type: "char(36)", nullable: false),
                    OwnerID = table.Column<Guid>(type: "char(36)", nullable: false),
                    BankNumber = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    Bank = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wallet", x => x.WalletID);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Competition",
                columns: table => new
                {
                    CompetitionID = table.Column<Guid>(type: "char(36)", nullable: false),
                    ClubID = table.Column<Guid>(type: "char(36)", nullable: false),
                    NameVN = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    NameEN = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    DescriptionVN = table.Column<string>(type: "text", nullable: true),
                    DescriptionEN = table.Column<string>(type: "text", nullable: true),
                    RuleContent = table.Column<string>(type: "text", nullable: false),
                    MaxParticipants = table.Column<int>(type: "int", nullable: true),
                    VisibleAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    RegistrationStartDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    RegistrationEndDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ResultPublishedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    IsSummarized = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    InvalidReason = table.Column<string>(type: "varchar(45)", nullable: true),
                    InvalidAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "char(36)", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "char(36)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Competition", x => x.CompetitionID);
                    table.ForeignKey(
                        name: "FK_Competition_Club_ClubID",
                        column: x => x.ClubID,
                        principalTable: "Club",
                        principalColumn: "ClubID",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Participation",
                columns: table => new
                {
                    ParticipationID = table.Column<Guid>(type: "char(36)", nullable: false),
                    UserID = table.Column<Guid>(type: "char(36)", nullable: false),
                    ClubID = table.Column<Guid>(type: "char(36)", nullable: false),
                    ApproverID = table.Column<Guid>(type: "char(36)", nullable: true),
                    Status = table.Column<sbyte>(type: "tinyint", nullable: false),
                    Note = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    JoinDate = table.Column<DateTime>(type: "datetime", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    LeftDate = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Participation", x => x.ParticipationID);
                    table.CheckConstraint("CK_Participation_Status", "`Status` IN (0, 1, 2)");
                    table.ForeignKey(
                        name: "FK_Participation_Club_ClubID",
                        column: x => x.ClubID,
                        principalTable: "Club",
                        principalColumn: "ClubID",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Media",
                columns: table => new
                {
                    MediaID = table.Column<Guid>(type: "char(36)", nullable: false),
                    MediaTypeID = table.Column<Guid>(type: "char(36)", nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Media", x => x.MediaID);
                    table.ForeignKey(
                        name: "FK_Media_MediaType_MediaTypeID",
                        column: x => x.MediaTypeID,
                        principalTable: "MediaType",
                        principalColumn: "MediaTypeID",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Product",
                columns: table => new
                {
                    ProductID = table.Column<Guid>(type: "char(36)", nullable: false),
                    ReferenceID = table.Column<Guid>(type: "char(36)", nullable: false),
                    CategoryID = table.Column<Guid>(type: "char(36)", nullable: true),
                    ProductNameVN = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    ProductNameEN = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    DescriptionVN = table.Column<string>(type: "text", nullable: false),
                    DescriptionEN = table.Column<string>(type: "text", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(20,9)", nullable: false),
                    Currency = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false, defaultValue: "VND"),
                    Status = table.Column<sbyte>(type: "tinyint", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "datetime", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    UpdateAt = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Product", x => x.ProductID);
                    table.CheckConstraint("CK_Product_Currency", "`Currency` IN ('VND', 'USD')");
                    table.CheckConstraint("CK_Product_Status", "`Status` IN (1, 2)");
                    table.ForeignKey(
                        name: "FK_Product_ProductCategory_CategoryID",
                        column: x => x.CategoryID,
                        principalTable: "ProductCategory",
                        principalColumn: "CategoryID",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Transaction",
                columns: table => new
                {
                    TransactionID = table.Column<Guid>(type: "char(36)", nullable: false),
                    WalletID = table.Column<Guid>(type: "char(36)", nullable: false),
                    Amount = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "varchar(20)", nullable: false),
                    Status = table.Column<sbyte>(type: "tinyint", nullable: false),
                    ReferenceID = table.Column<Guid>(type: "char(36)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transaction", x => x.TransactionID);
                    table.CheckConstraint("CK_Transaction_Status", "Status IN (0, 1, 2)");
                    table.CheckConstraint("CK_Transaction_Type", "Type IN ('COMMISSION', 'WITHDRAWAL', 'REFUND')");
                    table.ForeignKey(
                        name: "FK_Transaction_Wallet_WalletID",
                        column: x => x.WalletID,
                        principalTable: "Wallet",
                        principalColumn: "WalletID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CompetitionCertificate",
                columns: table => new
                {
                    CompetitionID = table.Column<Guid>(type: "char(36)", nullable: false),
                    CertificateID = table.Column<Guid>(type: "char(36)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompetitionCertificate", x => new { x.CompetitionID, x.CertificateID });
                    table.ForeignKey(
                        name: "FK_CompetitionCertificate_Competition_CompetitionID",
                        column: x => x.CompetitionID,
                        principalTable: "Competition",
                        principalColumn: "CompetitionID",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CompetitionPrize",
                columns: table => new
                {
                    CompetitionPrizeID = table.Column<Guid>(type: "char(36)", nullable: false),
                    CompetitionID = table.Column<Guid>(type: "char(36)", nullable: false),
                    TitleVN = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    TitleEN = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    DescriptionVN = table.Column<string>(type: "text", nullable: true),
                    DescriptionEN = table.Column<string>(type: "text", nullable: true),
                    RewardType = table.Column<sbyte>(type: "tinyint", nullable: false),
                    RewardValueGiftVN = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    RewardValueGiftEN = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    RewardValueMoney = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    RankFrom = table.Column<int>(type: "int", nullable: false),
                    RankTo = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "char(36)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "char(36)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompetitionPrize", x => x.CompetitionPrizeID);
                    table.ForeignKey(
                        name: "FK_CompetitionPrize_Competition_CompetitionID",
                        column: x => x.CompetitionID,
                        principalTable: "Competition",
                        principalColumn: "CompetitionID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Round",
                columns: table => new
                {
                    RoundID = table.Column<Guid>(type: "char(36)", nullable: false),
                    CompetitionID = table.Column<Guid>(type: "char(36)", nullable: false),
                    LabID = table.Column<Guid>(type: "char(36)", nullable: false),
                    RoundNumber = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    TimeLimit = table.Column<TimeSpan>(type: "time", nullable: false),
                    Status = table.Column<sbyte>(type: "tinyint", nullable: false),
                    IsSummarized = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "char(36)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "char(36)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Round", x => x.RoundID);
                    table.CheckConstraint("CK_Round_Status", "Status IN (0,1,2,3)");
                    table.ForeignKey(
                        name: "FK_Round_Competition_CompetitionID",
                        column: x => x.CompetitionID,
                        principalTable: "Competition",
                        principalColumn: "CompetitionID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserCompetition",
                columns: table => new
                {
                    UserCompetitionID = table.Column<Guid>(type: "char(36)", nullable: false),
                    UserID = table.Column<Guid>(type: "char(36)", nullable: false),
                    CompetitionID = table.Column<Guid>(type: "char(36)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    Rank = table.Column<int>(type: "int", nullable: true),
                    PrizeID = table.Column<Guid>(type: "char(36)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCompetition", x => x.UserCompetitionID);
                    table.ForeignKey(
                        name: "FK_UserCompetition_Competition_CompetitionID",
                        column: x => x.CompetitionID,
                        principalTable: "Competition",
                        principalColumn: "CompetitionID",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ClubAttemptRequest",
                columns: table => new
                {
                    ClubRequestID = table.Column<Guid>(type: "char(36)", nullable: false),
                    RequesterID = table.Column<Guid>(type: "char(36)", nullable: false),
                    ApproverID = table.Column<Guid>(type: "char(36)", nullable: true),
                    MediaID = table.Column<Guid>(type: "char(36)", nullable: true),
                    ClubID = table.Column<Guid>(type: "char(36)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClubAttemptRequest", x => x.ClubRequestID);
                    table.ForeignKey(
                        name: "FK_ClubAttemptRequest_Club_ClubID",
                        column: x => x.ClubID,
                        principalTable: "Club",
                        principalColumn: "ClubID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClubAttemptRequest_Media_MediaID",
                        column: x => x.MediaID,
                        principalTable: "Media",
                        principalColumn: "MediaID",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ClubCreationRequest",
                columns: table => new
                {
                    ClubCreationRequestID = table.Column<Guid>(type: "char(36)", nullable: false),
                    NameVN = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    NameEN = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    IsPublic = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    LimitParticipant = table.Column<int>(type: "int", nullable: false),
                    LimitClubManager = table.Column<int>(type: "int", nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: true),
                    MediaID = table.Column<Guid>(type: "char(36)", nullable: false),
                    ClubPolicy = table.Column<string>(type: "text", nullable: true),
                    DroneID = table.Column<Guid>(type: "char(36)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    RejectReason = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    ClubID = table.Column<Guid>(type: "char(36)", nullable: true),
                    RequesterID = table.Column<Guid>(type: "char(36)", nullable: false),
                    ApproverID = table.Column<Guid>(type: "char(36)", nullable: true),
                    Status = table.Column<byte>(type: "tinyint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClubCreationRequest", x => x.ClubCreationRequestID);
                    table.ForeignKey(
                        name: "FK_ClubCreationRequest_Club_ClubID",
                        column: x => x.ClubID,
                        principalTable: "Club",
                        principalColumn: "ClubID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClubCreationRequest_Media_MediaID",
                        column: x => x.MediaID,
                        principalTable: "Media",
                        principalColumn: "MediaID",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserProduct",
                columns: table => new
                {
                    UserProductID = table.Column<Guid>(type: "char(36)", nullable: false),
                    ProductID = table.Column<Guid>(type: "char(36)", nullable: false),
                    UserID = table.Column<Guid>(type: "char(36)", nullable: false),
                    AcquiredAt = table.Column<DateTime>(type: "datetime", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Source = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProduct", x => x.UserProductID);
                    table.ForeignKey(
                        name: "FK_UserProduct_Product_ProductID",
                        column: x => x.ProductID,
                        principalTable: "Product",
                        principalColumn: "ProductID",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserPrize",
                columns: table => new
                {
                    userPrizeID = table.Column<Guid>(type: "char(36)", nullable: false),
                    userID = table.Column<Guid>(type: "char(36)", nullable: false),
                    CompetitionID = table.Column<Guid>(type: "char(36)", nullable: false),
                    prizeID = table.Column<Guid>(type: "char(36)", nullable: false),
                    rank = table.Column<int>(type: "int", nullable: false),
                    rewardType = table.Column<int>(type: "int", nullable: false),
                    rewardValueMoney = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    rewardValueGiftVN = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    rewardValueGiftEN = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    isAwarded = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    awardedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    createdAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    createdBy = table.Column<Guid>(type: "char(36)", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    updatedBy = table.Column<Guid>(type: "char(36)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPrize", x => x.userPrizeID);
                    table.ForeignKey(
                        name: "FK_UserPrize_CompetitionPrize_prizeID",
                        column: x => x.prizeID,
                        principalTable: "CompetitionPrize",
                        principalColumn: "CompetitionPrizeID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserPrize_Competition_CompetitionID",
                        column: x => x.CompetitionID,
                        principalTable: "Competition",
                        principalColumn: "CompetitionID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserRound",
                columns: table => new
                {
                    UserRoundID = table.Column<Guid>(type: "char(36)", nullable: false),
                    UserID = table.Column<Guid>(type: "char(36)", nullable: false),
                    RoundID = table.Column<Guid>(type: "char(36)", nullable: false),
                    Solution = table.Column<string>(type: "text", nullable: true),
                    ExecutionTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    NumberOfSteps = table.Column<int>(type: "int", nullable: true),
                    PathLength = table.Column<float>(type: "float", nullable: true),
                    FeedbackVN = table.Column<string>(type: "text", nullable: true),
                    FeedbackEN = table.Column<string>(type: "text", nullable: true),
                    Rating = table.Column<int>(type: "int", nullable: true),
                    Point = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IsPassed = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    IsSequentialCheckpoints = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    Status = table.Column<sbyte>(type: "tinyint", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    Rank = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRound", x => x.UserRoundID);
                    table.ForeignKey(
                        name: "FK_UserRound_Round_RoundID",
                        column: x => x.RoundID,
                        principalTable: "Round",
                        principalColumn: "RoundID",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ClubAttemptRequest_ClubID",
                table: "ClubAttemptRequest",
                column: "ClubID");

            migrationBuilder.CreateIndex(
                name: "IX_ClubAttemptRequest_MediaID",
                table: "ClubAttemptRequest",
                column: "MediaID");

            migrationBuilder.CreateIndex(
                name: "IX_ClubAttemptRequest_RequesterID",
                table: "ClubAttemptRequest",
                column: "RequesterID");

            migrationBuilder.CreateIndex(
                name: "IX_ClubAttemptRequest_Status",
                table: "ClubAttemptRequest",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ClubCreationRequest_ClubID",
                table: "ClubCreationRequest",
                column: "ClubID");

            migrationBuilder.CreateIndex(
                name: "IX_ClubCreationRequest_MediaID",
                table: "ClubCreationRequest",
                column: "MediaID");

            migrationBuilder.CreateIndex(
                name: "IX_Competition_ClubID",
                table: "Competition",
                column: "ClubID");

            migrationBuilder.CreateIndex(
                name: "IX_CompetitionPrize_CompetitionID",
                table: "CompetitionPrize",
                column: "CompetitionID");

            migrationBuilder.CreateIndex(
                name: "IX_Media_MediaTypeID",
                table: "Media",
                column: "MediaTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_Participation_ClubID",
                table: "Participation",
                column: "ClubID");

            migrationBuilder.CreateIndex(
                name: "IX_Product_CategoryID",
                table: "Product",
                column: "CategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_Round_CompetitionID",
                table: "Round",
                column: "CompetitionID");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_WalletID",
                table: "Transaction",
                column: "WalletID");

            migrationBuilder.CreateIndex(
                name: "IX_UserCompetition_CompetitionID",
                table: "UserCompetition",
                column: "CompetitionID");

            migrationBuilder.CreateIndex(
                name: "IX_UserCompetition_UserID",
                table: "UserCompetition",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_UserPrize_CompetitionID",
                table: "UserPrize",
                column: "CompetitionID");

            migrationBuilder.CreateIndex(
                name: "IX_UserPrize_prizeID",
                table: "UserPrize",
                column: "prizeID");

            migrationBuilder.CreateIndex(
                name: "IX_UserProduct_ProductID",
                table: "UserProduct",
                column: "ProductID");

            migrationBuilder.CreateIndex(
                name: "IX_UserRound_RoundID",
                table: "UserRound",
                column: "RoundID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClubAttemptRequest");

            migrationBuilder.DropTable(
                name: "ClubCreationRequest");

            migrationBuilder.DropTable(
                name: "CompetitionCertificate");

            migrationBuilder.DropTable(
                name: "Participation");

            migrationBuilder.DropTable(
                name: "Transaction");

            migrationBuilder.DropTable(
                name: "UserCompetition");

            migrationBuilder.DropTable(
                name: "UserPrize");

            migrationBuilder.DropTable(
                name: "UserProduct");

            migrationBuilder.DropTable(
                name: "UserRound");

            migrationBuilder.DropTable(
                name: "Media");

            migrationBuilder.DropTable(
                name: "Wallet");

            migrationBuilder.DropTable(
                name: "CompetitionPrize");

            migrationBuilder.DropTable(
                name: "Product");

            migrationBuilder.DropTable(
                name: "Round");

            migrationBuilder.DropTable(
                name: "MediaType");

            migrationBuilder.DropTable(
                name: "ProductCategory");

            migrationBuilder.DropTable(
                name: "Competition");

            migrationBuilder.DropTable(
                name: "Club");
        }
    }
}
