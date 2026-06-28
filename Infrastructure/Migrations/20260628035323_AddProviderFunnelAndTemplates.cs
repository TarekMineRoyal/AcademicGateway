using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProviderFunnelAndTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "reviewers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    identity_user_id = table.Column<string>(type: "text", nullable: false),
                    full_name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_reviewers", x => x.id);
                    table.ForeignKey(
                        name: "fk_reviewers_asp_net_users_identity_user_id",
                        column: x => x.identity_user_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tech_support_accounts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider_id = table.Column<string>(type: "text", nullable: false),
                    identity_user_id = table.Column<string>(type: "text", nullable: false),
                    full_name = table.Column<string>(type: "text", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tech_support_accounts", x => x.id);
                    table.ForeignKey(
                        name: "fk_tech_support_accounts_asp_net_users_identity_user_id",
                        column: x => x.identity_user_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_tech_support_accounts_providers_provider_id",
                        column: x => x.provider_id,
                        principalTable: "providers",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "project_templates",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider_id = table.Column<string>(type: "text", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    expected_duration_weeks = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    approved_by_id = table.Column<Guid>(type: "uuid", nullable: true),
                    rejection_reason = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_project_templates", x => x.id);
                    table.ForeignKey(
                        name: "fk_project_templates_providers_provider_id",
                        column: x => x.provider_id,
                        principalTable: "providers",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_project_templates_reviewers_approved_by_id",
                        column: x => x.approved_by_id,
                        principalTable: "reviewers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "provider_applications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider_id = table.Column<string>(type: "text", nullable: false),
                    company_details = table.Column<string>(type: "text", nullable: false),
                    verification_documents_url = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    reviewed_by_id = table.Column<Guid>(type: "uuid", nullable: true),
                    rejection_reason = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    reviewed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_provider_applications", x => x.id);
                    table.ForeignKey(
                        name: "fk_provider_applications_providers_provider_id",
                        column: x => x.provider_id,
                        principalTable: "providers",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_provider_applications_reviewers_reviewed_by_id",
                        column: x => x.reviewed_by_id,
                        principalTable: "reviewers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "project_template_skills",
                columns: table => new
                {
                    project_template_id = table.Column<Guid>(type: "uuid", nullable: false),
                    skill_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_project_template_skills", x => new { x.project_template_id, x.skill_id });
                    table.ForeignKey(
                        name: "fk_project_template_skills_project_templates_project_template_",
                        column: x => x.project_template_id,
                        principalTable: "project_templates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_project_template_skills_skills_skill_id",
                        column: x => x.skill_id,
                        principalTable: "skills",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_project_template_skills_skill_id",
                table: "project_template_skills",
                column: "skill_id");

            migrationBuilder.CreateIndex(
                name: "ix_project_templates_approved_by_id",
                table: "project_templates",
                column: "approved_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_project_templates_provider_id",
                table: "project_templates",
                column: "provider_id");

            migrationBuilder.CreateIndex(
                name: "ix_provider_applications_provider_id",
                table: "provider_applications",
                column: "provider_id");

            migrationBuilder.CreateIndex(
                name: "ix_provider_applications_reviewed_by_id",
                table: "provider_applications",
                column: "reviewed_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_reviewers_identity_user_id",
                table: "reviewers",
                column: "identity_user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tech_support_accounts_identity_user_id",
                table: "tech_support_accounts",
                column: "identity_user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tech_support_accounts_provider_id",
                table: "tech_support_accounts",
                column: "provider_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "project_template_skills");

            migrationBuilder.DropTable(
                name: "provider_applications");

            migrationBuilder.DropTable(
                name: "tech_support_accounts");

            migrationBuilder.DropTable(
                name: "project_templates");

            migrationBuilder.DropTable(
                name: "reviewers");
        }
    }
}
