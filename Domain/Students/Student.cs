using System;
using System.Collections.Generic;
using System.Linq;
using AcademicGateway.Domain.Common;
using AcademicGateway.Domain.Students.Exceptions;

namespace AcademicGateway.Domain.Students;

/// <summary>
/// Represents a Student aggregate root profile within the academic gateway.
/// Enforces business rule properties over scholastic identities, degree mappings, and competency matrices.
/// </summary>
public class Student : BaseEntity
{
    private readonly List<StudentMajor> _studentMajors = new();
    private readonly List<StudentSkill> _studentSkills = new();
    private readonly List<StudentSpecialty> _studentSpecialties = new();

    /// <summary>
    /// Gets the unique identifier for the Student profile. 
    /// Maps 1:1 to the underlying Identity ApplicationUser identifier.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the legal full display name of the student.
    /// </summary>
    public string FullName { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the optional targeted graduation completion calendar year logged by the student.
    /// </summary>
    public int? GraduationYear { get; private set; }

    /// <summary>
    /// Gets a read-only encapsulated list tracking academic major alignments chosen by this student.
    /// </summary>
    public IReadOnlyCollection<StudentMajor> StudentMajors => _studentMajors.AsReadOnly();

    /// <summary>
    /// Gets a read-only encapsulated list tracking technical skill sets claimed by this student.
    /// </summary>
    public IReadOnlyCollection<StudentSkill> StudentSkills => _studentSkills.AsReadOnly();

    /// <summary>
    /// Gets a read-only encapsulated list tracking minor academic concentrations chosen by this student.
    /// </summary>
    public IReadOnlyCollection<StudentSpecialty> StudentSpecialties => _studentSpecialties.AsReadOnly();

    /// <summary>
    /// Required parameterless constructor variant for Entity Framework Core relational database hydration mappings.
    /// </summary>
    private Student()
    {
    }

    /// <summary>
    /// Initializes a new valid domain instance of the <see cref="Student"/> aggregate root.
    /// </summary>
    /// <param name="id">The unique Identity key linking back to user authentication credentials.</param>
    /// <param name="fullName">The full legal identity name tracking string.</param>
    /// <param name="graduationYear">The expected completion target year parameters.</param>
    /// <exception cref="InvalidStudentDetailsException">Thrown if identity string formatting rules fail invariants.</exception>
    /// <exception cref="InvalidGraduationYearException">Thrown if chronological dates fall beneath system baselines.</exception>
    public Student(Guid id, string fullName, int? graduationYear = null)
    {
        if (id == Guid.Empty)
        {
            throw new InvalidStudentDetailsException("Identity User ID reference context cannot be empty.");
        }

        Id = id;
        UpdateFullName(fullName);
        UpdateGraduationYear(graduationYear);
    }

    /// <summary>
    /// Mutates the full display identity name text saved on the profile database structures.
    /// </summary>
    /// <param name="newFullName">The target legal or professional string parameter to assert.</param>
    /// <exception cref="InvalidStudentDetailsException">Thrown if the string argument resolves to blank or null values.</exception>
    public void UpdateFullName(string newFullName)
    {
        if (string.IsNullOrWhiteSpace(newFullName))
        {
            throw new InvalidStudentDetailsException("Student identity name fields cannot be empty or whitespace.");
        }

        FullName = newFullName.Trim();
    }

    /// <summary>
    /// Adjusts the target completion date fields tracked for this student record.
    /// </summary>
    /// <param name="graduationYear">The targeted calendar year assignment values.</param>
    /// <exception cref="InvalidGraduationYearException">Thrown if a non-null year drops below the baseline index marker.</exception>
    public void UpdateGraduationYear(int? graduationYear)
    {
        if (graduationYear.HasValue && graduationYear.Value < 2000)
        {
            throw new InvalidGraduationYearException(graduationYear.Value);
        }

        GraduationYear = graduationYear;
    }

    /// <summary>
    /// Maps a formal academic major path allocation straight to this Student aggregate registry tracking boundary.
    /// </summary>
    /// <param name="majorId">The unique surrogate reference identifier tracking the root major lookup entity.</param>
    public void AddMajor(Guid majorId)
    {
        if (majorId == Guid.Empty)
        {
            throw new InvalidStudentDetailsException("Target reference major identification context cannot be empty.");
        }

        if (_studentMajors.Any(sm => sm.MajorId == majorId))
        {
            return;
        }

        _studentMajors.Add(new StudentMajor(Id, majorId));
    }

    /// <summary>
    /// Drops an established major tracking alignment context row straight from this student profile.
    /// </summary>
    /// <param name="majorId">The surrogate tracking key candidates targeted for deletion.</param>
    public void RemoveMajor(Guid majorId)
    {
        var majorMapping = _studentMajors.FirstOrDefault(sm => sm.MajorId == majorId);
        if (majorMapping != null)
        {
            _studentMajors.Remove(majorMapping);
        }
    }

    /// <summary>
    /// Attaches an evaluated technical capability or professional skill keyword identity reference straight onto the student.
    /// </summary>
    /// <param name="skillId">The primary identification key targeting the universal skill asset lookup row.</param>
    public void AddSkill(Guid skillId)
    {
        if (skillId == Guid.Empty)
        {
            throw new InvalidStudentDetailsException("Target reference skill identification context cannot be empty.");
        }

        if (_studentSkills.Any(ss => ss.SkillId == skillId))
        {
            return;
        }

        _studentSkills.Add(new StudentSkill(Id, skillId));
    }

    /// <summary>
    /// Isolates and deletes an active competency skill classification mapping link out of the profile structures.
    /// </summary>
    /// <param name="skillId">The unique key identifying the item targeting for drop isolation processing.</param>
    public void RemoveSkill(Guid skillId)
    {
        var skillMapping = _studentSkills.FirstOrDefault(ss => ss.SkillId == skillId);
        if (skillMapping != null)
        {
            _studentSkills.Remove(skillMapping);
        }
    }

    /// <summary>
    /// Appends a technical minor concentration track specialty route selection choice configuration onto the profile metrics.
    /// </summary>
    /// <param name="specialtyId">The unique key identifying the tracking sub-track parameter mapping reference row.</param>
    public void AddSpecialty(Guid specialtyId)
    {
        if (specialtyId == Guid.Empty)
        {
            throw new InvalidStudentDetailsException("Target reference sub-track specialty identification context cannot be empty.");
        }

        if (_studentSpecialties.Any(ss => ss.SpecialtyId == specialtyId))
        {
            return;
        }

        _studentSpecialties.Add(new StudentSpecialty(Id, specialtyId));
    }

    /// <summary>
    /// Clears an assigned sub-concentration track matrix assignment row configuration mapping context out of the student boundary.
    /// </summary>
    /// <param name="specialtyId">The tracking identity surrogate candidate marker to erase.</param>
    public void RemoveSpecialty(Guid specialtyId)
    {
        var specialtyMapping = _studentSpecialties.FirstOrDefault(ss => ss.SpecialtyId == specialtyId);
        if (specialtyMapping != null)
        {
            _studentSpecialties.Remove(specialtyMapping);
        }
    }
}