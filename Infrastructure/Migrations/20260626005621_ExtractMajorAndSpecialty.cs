using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ExtractMajorAndSpecialty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "major",
                table: "students");

            migrationBuilder.DropColumn(
                name: "specialty",
                table: "students");

            migrationBuilder.CreateTable(
                name: "majors",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_majors", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "specialties",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    major_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_specialties", x => x.id);
                    table.ForeignKey(
                        name: "fk_specialties_majors_major_id",
                        column: x => x.major_id,
                        principalTable: "majors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "student_majors",
                columns: table => new
                {
                    student_id = table.Column<string>(type: "text", nullable: false),
                    major_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_student_majors", x => new { x.student_id, x.major_id });
                    table.ForeignKey(
                        name: "fk_student_majors_majors_major_id",
                        column: x => x.major_id,
                        principalTable: "majors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_student_majors_students_student_id",
                        column: x => x.student_id,
                        principalTable: "students",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "student_specialties",
                columns: table => new
                {
                    student_id = table.Column<string>(type: "text", nullable: false),
                    specialty_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_student_specialties", x => new { x.student_id, x.specialty_id });
                    table.ForeignKey(
                        name: "fk_student_specialties_specialties_specialty_id",
                        column: x => x.specialty_id,
                        principalTable: "specialties",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_student_specialties_students_student_id",
                        column: x => x.student_id,
                        principalTable: "students",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_specialties_major_id",
                table: "specialties",
                column: "major_id");

            migrationBuilder.CreateIndex(
                name: "ix_student_majors_major_id",
                table: "student_majors",
                column: "major_id");

            migrationBuilder.CreateIndex(
                name: "ix_student_specialties_specialty_id",
                table: "student_specialties",
                column: "specialty_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "student_majors");

            migrationBuilder.DropTable(
                name: "student_specialties");

            migrationBuilder.DropTable(
                name: "specialties");

            migrationBuilder.DropTable(
                name: "majors");

            migrationBuilder.AddColumn<string>(
                name: "major",
                table: "students",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "specialty",
                table: "students",
                type: "text",
                nullable: true);
        }
    }
}
