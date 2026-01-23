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
                name: "Category",
                columns: table => new
                {
                    CategoryID = table.Column<Guid>(type: "char(36)", nullable: false),
                    TypeNameVN = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    TypeNameEN = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    DescriptionVN = table.Column<string>(type: "text", nullable: false),
                    DescriptionEN = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Category", x => x.CategoryID);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Club",
                columns: table => new
                {
                    ClubID = table.Column<Guid>(type: "char(36)", nullable: false),
                    CreateBy = table.Column<Guid>(type: "char(36)", nullable: false),
                    NameVN = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    NameEN = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    DescriptionVN = table.Column<string>(type: "text", nullable: false),
                    DescriptionEN = table.Column<string>(type: "text", nullable: false),
                    ClubCode = table.Column<Guid>(type: "char(36)", nullable: false),
                    Status = table.Column<sbyte>(type: "tinyint", nullable: false),
                    IsPublic = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    LimitParticipation = table.Column<int>(type: "int", nullable: false),
                    LimitClubManagers = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Club", x => x.ClubID);
                    table.CheckConstraint("CK_Club_Status", "`Status` IN (0, 1)");
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
                    CreateAt = table.Column<DateTime>(type: "datetime", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    UpdateAt = table.Column<DateTime>(type: "datetime", nullable: false)
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
                name: "ClubCategory",
                columns: table => new
                {
                    CategoryID = table.Column<Guid>(type: "char(36)", nullable: false),
                    ClubID = table.Column<Guid>(type: "char(36)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClubCategory", x => new { x.CategoryID, x.ClubID });
                    table.ForeignKey(
                        name: "FK_ClubCategory_Category_CategoryID",
                        column: x => x.CategoryID,
                        principalTable: "Category",
                        principalColumn: "CategoryID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClubCategory_Club_ClubID",
                        column: x => x.ClubID,
                        principalTable: "Club",
                        principalColumn: "ClubID",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ClubCourse",
                columns: table => new
                {
                    ClubID = table.Column<Guid>(type: "char(36)", nullable: false),
                    CourseID = table.Column<Guid>(type: "char(36)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClubCourse", x => new { x.ClubID, x.CourseID });
                    table.ForeignKey(
                        name: "FK_ClubCourse_Club_ClubID",
                        column: x => x.ClubID,
                        principalTable: "Club",
                        principalColumn: "ClubID",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ClubRequest",
                columns: table => new
                {
                    ClubRequestID = table.Column<Guid>(type: "char(36)", nullable: false),
                    RequestID = table.Column<Guid>(type: "char(36)", nullable: false),
                    ApproveID = table.Column<Guid>(type: "char(36)", nullable: false),
                    ClubID = table.Column<Guid>(type: "char(36)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClubRequest", x => x.ClubRequestID);
                    table.ForeignKey(
                        name: "FK_ClubRequest_Club_ClubID",
                        column: x => x.ClubID,
                        principalTable: "Club",
                        principalColumn: "ClubID",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Competition",
                columns: table => new
                {
                    CompetitionID = table.Column<Guid>(type: "char(36)", nullable: false),
                    ClubID = table.Column<Guid>(type: "char(36)", nullable: false),
                    CreateBy = table.Column<Guid>(type: "char(36)", nullable: false),
                    NameVN = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    NameEN = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    DescriptionVN = table.Column<string>(type: "text", nullable: false),
                    DescriptionEN = table.Column<string>(type: "text", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime", nullable: false)
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
                    ApproveID = table.Column<Guid>(type: "char(36)", nullable: false),
                    ClubID = table.Column<Guid>(type: "char(36)", nullable: false),
                    Status = table.Column<ulong>(type: "bit", nullable: false),
                    JoinDate = table.Column<DateTime>(type: "datetime", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Participation", x => x.ParticipationID);
                    table.CheckConstraint("CK_Participation_Status", "`Status` IN (0, 1)");
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
                    CreateAt = table.Column<DateTime>(type: "datetime", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    UpdateAt = table.Column<DateTime>(type: "datetime", nullable: false)
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
                    CodeID = table.Column<Guid>(type: "char(36)", nullable: false),
                    ReferenceID = table.Column<Guid>(type: "char(36)", nullable: false),
                    CategoryID = table.Column<Guid>(type: "char(36)", nullable: false),
                    ProductNameVN = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    ProductNameEN = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    DescriptionVN = table.Column<string>(type: "text", nullable: false),
                    DescriptionEN = table.Column<string>(type: "text", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(14,9)", nullable: false),
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
                name: "Round",
                columns: table => new
                {
                    RoundID = table.Column<Guid>(type: "char(36)", nullable: false),
                    CompetitionID = table.Column<Guid>(type: "char(36)", nullable: false),
                    LabID = table.Column<Guid>(type: "char(36)", nullable: false),
                    RoundNumber = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Round", x => x.RoundID);
                    table.CheckConstraint("CK_Round_Status", "Status IN (0, 1)");
                    table.ForeignKey(
                        name: "FK_Round_Competition_CompetitionID",
                        column: x => x.CompetitionID,
                        principalTable: "Competition",
                        principalColumn: "CompetitionID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserCompetion",
                columns: table => new
                {
                    UserID = table.Column<Guid>(type: "char(36)", nullable: false),
                    CompetitionID = table.Column<Guid>(type: "char(36)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCompetion", x => new { x.UserID, x.CompetitionID });
                    table.ForeignKey(
                        name: "FK_UserCompetion_Competition_CompetitionID",
                        column: x => x.CompetitionID,
                        principalTable: "Competition",
                        principalColumn: "CompetitionID",
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
                    Status = table.Column<string>(type: "longtext", nullable: false)
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
                name: "UserRound",
                columns: table => new
                {
                    UserRoundID = table.Column<Guid>(type: "char(36)", nullable: false),
                    UserID = table.Column<Guid>(type: "char(36)", nullable: false),
                    RoundID = table.Column<Guid>(type: "char(36)", nullable: false),
                    Solution = table.Column<string>(type: "text", nullable: false),
                    IsCompleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Time = table.Column<float>(type: "float", nullable: false),
                    NumberOfStep = table.Column<int>(type: "int", nullable: false),
                    Length = table.Column<float>(type: "float", nullable: false),
                    FeedbackVN = table.Column<string>(type: "text", nullable: false),
                    FeedbackEN = table.Column<string>(type: "text", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    Point = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
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
                name: "IX_ClubCategory_ClubID",
                table: "ClubCategory",
                column: "ClubID");

            migrationBuilder.CreateIndex(
                name: "IX_ClubRequest_ClubID",
                table: "ClubRequest",
                column: "ClubID");

            migrationBuilder.CreateIndex(
                name: "IX_Competition_ClubID",
                table: "Competition",
                column: "ClubID");

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
                name: "IX_UserCompetion_CompetitionID",
                table: "UserCompetion",
                column: "CompetitionID");

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
                name: "ClubCategory");

            migrationBuilder.DropTable(
                name: "ClubCourse");

            migrationBuilder.DropTable(
                name: "ClubRequest");

            migrationBuilder.DropTable(
                name: "CompetitionCertificate");

            migrationBuilder.DropTable(
                name: "Media");

            migrationBuilder.DropTable(
                name: "Participation");

            migrationBuilder.DropTable(
                name: "UserCompetion");

            migrationBuilder.DropTable(
                name: "UserProduct");

            migrationBuilder.DropTable(
                name: "UserRound");

            migrationBuilder.DropTable(
                name: "Category");

            migrationBuilder.DropTable(
                name: "MediaType");

            migrationBuilder.DropTable(
                name: "Product");

            migrationBuilder.DropTable(
                name: "Round");

            migrationBuilder.DropTable(
                name: "ProductCategory");

            migrationBuilder.DropTable(
                name: "Competition");

            migrationBuilder.DropTable(
                name: "Club");
        }
    }
}
