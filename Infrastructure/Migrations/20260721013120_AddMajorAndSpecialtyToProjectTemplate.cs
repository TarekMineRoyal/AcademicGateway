using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMajorAndSpecialtyToProjectTemplate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "major_id",
                table: "ProjectTemplates",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "specialty_id",
                table: "ProjectTemplates",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_project_templates_major_id",
                table: "ProjectTemplates",
                column: "major_id");

            migrationBuilder.CreateIndex(
                name: "ix_project_templates_specialty_id",
                table: "ProjectTemplates",
                column: "specialty_id");

            migrationBuilder.AddForeignKey(
                name: "fk_project_templates_majors_major_id",
                table: "ProjectTemplates",
                column: "major_id",
                principalTable: "Majors",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_project_templates_specialties_specialty_id",
                table: "ProjectTemplates",
                column: "specialty_id",
                principalTable: "Specialties",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_project_templates_majors_major_id",
                table: "ProjectTemplates");

            migrationBuilder.DropForeignKey(
                name: "fk_project_templates_specialties_specialty_id",
                table: "ProjectTemplates");

            migrationBuilder.DropIndex(
                name: "ix_project_templates_major_id",
                table: "ProjectTemplates");

            migrationBuilder.DropIndex(
                name: "ix_project_templates_specialty_id",
                table: "ProjectTemplates");

            migrationBuilder.DropColumn(
                name: "major_id",
                table: "ProjectTemplates");

            migrationBuilder.DropColumn(
                name: "specialty_id",
                table: "ProjectTemplates");
        }
    }
}
