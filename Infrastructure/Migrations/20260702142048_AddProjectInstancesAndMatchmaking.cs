using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectInstancesAndMatchmaking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProjectInstances",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    supervisor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    template_id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title_snapshot = table.Column<string>(type: "text", nullable: false),
                    description_snapshot = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_project_instances", x => x.id);
                    table.ForeignKey(
                        name: "fk_project_instances_professors_supervisor_id",
                        column: x => x.supervisor_id,
                        principalTable: "Professors",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_project_instances_students_student_id",
                        column: x => x.student_id,
                        principalTable: "Students",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectInstanceSkills",
                columns: table => new
                {
                    project_instance_id = table.Column<Guid>(type: "uuid", nullable: false),
                    skill_id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_instance_id1 = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_project_instance_skills", x => new { x.project_instance_id, x.skill_id });
                    table.ForeignKey(
                        name: "fk_project_instance_skills_project_instances_project_instance_id",
                        column: x => x.project_instance_id,
                        principalTable: "ProjectInstances",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_project_instance_skills_project_instances_project_instance_id1",
                        column: x => x.project_instance_id1,
                        principalTable: "ProjectInstances",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_project_instance_skills_skills_skill_id",
                        column: x => x.skill_id,
                        principalTable: "Skills",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SupervisionRequests",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_instance_id = table.Column<Guid>(type: "uuid", nullable: false),
                    professor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    pitch_text = table.Column<string>(type: "character varying(1500)", maxLength: 1500, nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    rejection_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    reviewed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    project_instance_id1 = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_supervision_requests", x => x.id);
                    table.ForeignKey(
                        name: "fk_supervision_requests_professors_professor_id",
                        column: x => x.professor_id,
                        principalTable: "Professors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_supervision_requests_project_instances_project_instance_id",
                        column: x => x.project_instance_id,
                        principalTable: "ProjectInstances",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_supervision_requests_project_instances_project_instance_id1",
                        column: x => x.project_instance_id1,
                        principalTable: "ProjectInstances",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TechSupportProposals",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_instance_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tech_support_account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    rejection_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    project_instance_id1 = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tech_support_proposals", x => x.id);
                    table.ForeignKey(
                        name: "fk_tech_support_proposals_project_instances_project_instance_id",
                        column: x => x.project_instance_id,
                        principalTable: "ProjectInstances",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_tech_support_proposals_project_instances_project_instance_id1",
                        column: x => x.project_instance_id1,
                        principalTable: "ProjectInstances",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_tech_support_proposals_tech_support_accounts_tech_support_accou",
                        column: x => x.tech_support_account_id,
                        principalTable: "TechSupportAccounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_project_instances_student_id",
                table: "ProjectInstances",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "ix_project_instances_supervisor_id",
                table: "ProjectInstances",
                column: "supervisor_id");

            migrationBuilder.CreateIndex(
                name: "ix_project_instance_skills_project_instance_id1",
                table: "ProjectInstanceSkills",
                column: "project_instance_id1");

            migrationBuilder.CreateIndex(
                name: "ix_project_instance_skills_skill_id",
                table: "ProjectInstanceSkills",
                column: "skill_id");

            migrationBuilder.CreateIndex(
                name: "ix_supervision_requests_professor_id",
                table: "SupervisionRequests",
                column: "professor_id");

            migrationBuilder.CreateIndex(
                name: "ix_supervision_requests_project_instance_id",
                table: "SupervisionRequests",
                column: "project_instance_id");

            migrationBuilder.CreateIndex(
                name: "ix_supervision_requests_project_instance_id1",
                table: "SupervisionRequests",
                column: "project_instance_id1");

            migrationBuilder.CreateIndex(
                name: "ix_tech_support_proposals_project_instance_id",
                table: "TechSupportProposals",
                column: "project_instance_id");

            migrationBuilder.CreateIndex(
                name: "ix_tech_support_proposals_project_instance_id1",
                table: "TechSupportProposals",
                column: "project_instance_id1");

            migrationBuilder.CreateIndex(
                name: "ix_tech_support_proposals_tech_support_account_id",
                table: "TechSupportProposals",
                column: "tech_support_account_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectInstanceSkills");

            migrationBuilder.DropTable(
                name: "SupervisionRequests");

            migrationBuilder.DropTable(
                name: "TechSupportProposals");

            migrationBuilder.DropTable(
                name: "ProjectInstances");
        }
    }
}
