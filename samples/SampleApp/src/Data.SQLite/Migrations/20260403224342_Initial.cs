using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ploch.Data.SampleApp.Data.SQLite.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ArticleCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ParentId = table.Column<int>(type: "INTEGER", nullable: true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticleCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArticleCategories_ArticleCategories_ParentId",
                        column: x => x.ParentId,
                        principalTable: "ArticleCategories",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ArticleTags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticleTags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Authors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    CreatedTime = table.Column<long>(type: "INTEGER", nullable: true),
                    ModifiedTime = table.Column<long>(type: "INTEGER", nullable: true),
                    AccessedTime = table.Column<long>(type: "INTEGER", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "TEXT", nullable: true),
                    LastAccessedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Authors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Articles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: true),
                    Contents = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedTime = table.Column<long>(type: "INTEGER", nullable: true),
                    ModifiedTime = table.Column<long>(type: "INTEGER", nullable: true),
                    AccessedTime = table.Column<long>(type: "INTEGER", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "TEXT", nullable: true),
                    LastAccessedBy = table.Column<string>(type: "TEXT", nullable: true),
                    AuthorId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Articles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Articles_Authors_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Authors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ArticleArticleCategory",
                columns: table => new
                {
                    ArticlesId = table.Column<int>(type: "INTEGER", nullable: false),
                    CategoriesId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticleArticleCategory", x => new { x.ArticlesId, x.CategoriesId });
                    table.ForeignKey(
                        name: "FK_ArticleArticleCategory_ArticleCategories_CategoriesId",
                        column: x => x.CategoriesId,
                        principalTable: "ArticleCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArticleArticleCategory_Articles_ArticlesId",
                        column: x => x.ArticlesId,
                        principalTable: "Articles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArticleArticleTag",
                columns: table => new
                {
                    ArticlesId = table.Column<int>(type: "INTEGER", nullable: false),
                    TagsId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticleArticleTag", x => new { x.ArticlesId, x.TagsId });
                    table.ForeignKey(
                        name: "FK_ArticleArticleTag_ArticleTags_TagsId",
                        column: x => x.TagsId,
                        principalTable: "ArticleTags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArticleArticleTag_Articles_ArticlesId",
                        column: x => x.ArticlesId,
                        principalTable: "Articles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArticleProperties",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: false),
                    ArticleId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticleProperties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArticleProperties_Articles_ArticleId",
                        column: x => x.ArticleId,
                        principalTable: "Articles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArticleArticleCategory_CategoriesId",
                table: "ArticleArticleCategory",
                column: "CategoriesId");

            migrationBuilder.CreateIndex(
                name: "IX_ArticleArticleTag_TagsId",
                table: "ArticleArticleTag",
                column: "TagsId");

            migrationBuilder.CreateIndex(
                name: "IX_ArticleCategories_ParentId",
                table: "ArticleCategories",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_ArticleProperties_ArticleId",
                table: "ArticleProperties",
                column: "ArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_Articles_AuthorId",
                table: "Articles",
                column: "AuthorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArticleArticleCategory");

            migrationBuilder.DropTable(
                name: "ArticleArticleTag");

            migrationBuilder.DropTable(
                name: "ArticleProperties");

            migrationBuilder.DropTable(
                name: "ArticleCategories");

            migrationBuilder.DropTable(
                name: "ArticleTags");

            migrationBuilder.DropTable(
                name: "Articles");

            migrationBuilder.DropTable(
                name: "Authors");
        }
    }
}
