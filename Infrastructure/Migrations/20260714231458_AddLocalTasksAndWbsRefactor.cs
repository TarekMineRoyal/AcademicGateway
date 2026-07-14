using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLocalTasksAndWbsRefactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_global_milestone_project_templates_project_template_id",
                table: "global_milestone");

            migrationBuilder.DropForeignKey(
                name: "fk_template_milestone_dependencies_global_milestone_global_miles",
                table: "TemplateMilestoneDependencies");

            migrationBuilder.DropPrimaryKey(
                name: "pk_global_milestone",
                table: "global_milestone");

            migrationBuilder.DropColumn(
                name: "evaluation_feedback",
                table: "LocalMilestones");

            migrationBuilder.DropColumn(
                name: "grade",
                table: "LocalMilestones");

            migrationBuilder.DropColumn(
                name: "graded_at",
                table: "LocalMilestones");

            migrationBuilder.DropColumn(
                name: "required_deliverable_type",
                table: "LocalMilestones");

            migrationBuilder.DropColumn(
                name: "submission_payload",
                table: "LocalMilestones");

            migrationBuilder.DropColumn(
                name: "submitted_at",
                table: "LocalMilestones");

            migrationBuilder.DropColumn(
                name: "required_deliverable_type",
                table: "global_milestone");

            migrationBuilder.RenameTable(
                name: "global_milestone",
                newName: "GlobalMilestones");

            migrationBuilder.RenameIndex(
                name: "ix_global_milestone_project_template_id",
                table: "GlobalMilestones",
                newName: "IX_GlobalMilestones_ProjectTemplateId");

            migrationBuilder.AddColumn<Guid>(
                name: "provider_id",
                table: "TechSupportAccounts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<decimal>(
                name: "grading_weight",
                table: "LocalMilestones",
                type: "numeric(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "wbs_weight",
                table: "LocalMilestones",
                type: "numeric(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "title",
                table: "GlobalMilestones",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "GlobalMilestones",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<decimal>(
                name: "grading_weight",
                table: "GlobalMilestones",
                type: "numeric(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "wbs_weight",
                table: "GlobalMilestones",
                type: "numeric(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddPrimaryKey(
                name: "pk_global_milestones",
                table: "GlobalMilestones",
                column: "id");

            migrationBuilder.CreateTable(
                name: "GlobalTasks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    global_milestone_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    weight = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    required_deliverable_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_global_tasks", x => x.id);
                    table.ForeignKey(
                        name: "fk_global_tasks_global_milestones_global_milestone_id",
                        column: x => x.global_milestone_id,
                        principalTable: "GlobalMilestones",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LocalTasks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    local_milestone_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title_snapshot = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description_snapshot = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    weight = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    required_deliverable_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    submission_payload = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    submitted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    grade = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    evaluation_feedback = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    graded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_local_tasks", x => x.id);
                    table.ForeignKey(
                        name: "fk_local_tasks_local_milestones_local_milestone_id",
                        column: x => x.local_milestone_id,
                        principalTable: "LocalMilestones",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_global_tasks_global_milestone_id",
                table: "GlobalTasks",
                column: "global_milestone_id");

            migrationBuilder.CreateIndex(
                name: "IX_LocalTasks_LocalMilestoneId",
                table: "LocalTasks",
                column: "local_milestone_id");

            migrationBuilder.AddForeignKey(
                name: "fk_global_milestones_project_templates_project_template_id",
                table: "GlobalMilestones",
                column: "project_template_id",
                principalTable: "ProjectTemplates",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_template_milestone_dependencies_global_milestones_global_miles",
                table: "TemplateMilestoneDependencies",
                column: "global_milestone_id",
                principalTable: "GlobalMilestones",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_global_milestones_project_templates_project_template_id",
                table: "GlobalMilestones");

            migrationBuilder.DropForeignKey(
                name: "fk_template_milestone_dependencies_global_milestones_global_miles",
                table: "TemplateMilestoneDependencies");

            migrationBuilder.DropTable(
                name: "GlobalTasks");

            migrationBuilder.DropTable(
                name: "LocalTasks");

            migrationBuilder.DropPrimaryKey(
                name: "pk_global_milestones",
                table: "GlobalMilestones");

            migrationBuilder.DropColumn(
                name: "provider_id",
                table: "TechSupportAccounts");

            migrationBuilder.DropColumn(
                name: "grading_weight",
                table: "LocalMilestones");

            migrationBuilder.DropColumn(
                name: "wbs_weight",
                table: "LocalMilestones");

            migrationBuilder.DropColumn(
                name: "grading_weight",
                table: "GlobalMilestones");

            migrationBuilder.DropColumn(
                name: "wbs_weight",
                table: "GlobalMilestones");

            migrationBuilder.RenameTable(
                name: "GlobalMilestones",
                newName: "global_milestone");

            migrationBuilder.RenameIndex(
                name: "IX_GlobalMilestones_ProjectTemplateId",
                table: "global_milestone",
                newName: "ix_global_milestone_project_template_id");

            migrationBuilder.AddColumn<string>(
                name: "evaluation_feedback",
                table: "LocalMilestones",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "grade",
                table: "LocalMilestones",
                type: "numeric(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "graded_at",
                table: "LocalMilestones",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "required_deliverable_type",
                table: "LocalMilestones",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "submission_payload",
                table: "LocalMilestones",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "submitted_at",
                table: "LocalMilestones",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "title",
                table: "global_milestone",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "global_milestone",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000);

            migrationBuilder.AddColumn<int>(
                name: "required_deliverable_type",
                table: "global_milestone",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "pk_global_milestone",
                table: "global_milestone",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_global_milestone_project_templates_project_template_id",
                table: "global_milestone",
                column: "project_template_id",
                principalTable: "ProjectTemplates",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_template_milestone_dependencies_global_milestone_global_miles",
                table: "TemplateMilestoneDependencies",
                column: "global_milestone_id",
                principalTable: "global_milestone",
                principalColumn: "id");
        }
    }
}
