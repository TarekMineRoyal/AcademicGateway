using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Common;

namespace Domain.Students;

/// <summary>
/// Represents a student profile within the gateway tracking academic alignments, 
/// specialized tracks, technical skill sets, and program metrics.
/// </summary>
public class Student : BaseEntity
{
    private readonly List<StudentMajor> _studentMajors = new();
    private readonly List<StudentSpecialty> _studentSpecialties = new();
    private readonly List<StudentSkill> _studentSkills = new();

    /// <summary>
    /// Gets the unique identifier for the Student profile. 
    /// Maps 1:1 to the underlying Identity ApplicationUser identifier.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the targeted calendar year of graduation, if declared.
    /// </summary>
    public int? GraduationYear { get; private set; }

    /// <summary>
    /// Gets the read-only collaborative collection tracking the academic majors assigned to this student.
    /// </summary>
    public IReadOnlyCollection<StudentMajor> StudentMajors => _studentMajors.AsReadOnly();

    /// <summary>
    /// Gets the read-only collaborative collection tracking sub-specialty focuses assigned to this student.
    /// </summary>
    public IReadOnlyCollection<StudentSpecialty> StudentSpecialties => _studentSpecialties.AsReadOnly();

    /// <summary>
    /// Gets the read-only collaborative collection tracking professional skill competencies mapped to this student.
    /// </summary>
    public IReadOnlyCollection<StudentSkill> StudentSkills => _studentSkills.AsReadOnly();

    /// <summary>
    /// EF Core constructor requirement. Prevents bypass of domain constraints during persistence hydration.
    /// </summary>
    private Student()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Student"/> profile with validation boundaries.
    /// </summary>
    /// <param name="id">The unique Identity key linking back to the account credentials.</param>
    /// <param name="graduationYear">The expected calendar year of graduation.</param>
    /// <exception cref="ArgumentException">Thrown when the identity tracker reference is invalid.</exception>
    public Student(Guid id, int? graduationYear = null)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Identity User ID cannot be empty.", nameof(id));
        }

        Id = id;
        UpdateGraduationYear(graduationYear);
    }

    /// <summary>
    /// Updates the target completion year metrics, guarding against impossible historical values.
    /// </summary>
    /// <param name="graduationYear">The targeted calendar year of graduation.</param>
    /// <exception cref="ArgumentException">Thrown if an unrealistic historical year boundary is violated.</exception>
    public void UpdateGraduationYear(int? graduationYear)
    {
        if (graduationYear.HasValue && graduationYear.Value < 2000)
        {
            throw new ArgumentException("Graduation year cannot be an impossible historical date prior to 2000.", nameof(graduationYear));
        }

        GraduationYear = graduationYear;
    }

    /// <summary>
    /// Maps an institutional academic major onto this student's profile.
    /// </summary>
    /// <param name="majorId">The lookups reference key targeting the academic major.</param>
    public void AddMajor(Guid majorId)
    {
        if (majorId == Guid.Empty)
        {
            throw new ArgumentException("Major ID cannot be an empty Guid.", nameof(majorId));
        }

        if (_studentMajors.Any(sm => sm.MajorId == majorId))
        {
            return; // Association already exists
        }

        _studentMajors.Add(new StudentMajor(Id, majorId));
    }

    /// <summary>
    /// Unmaps an existing academic major classification from this student's profile.
    /// </summary>
    /// <param name="majorId">The lookups reference tracking key to disconnect.</param>
    public void RemoveMajor(Guid majorId)
    {
        var majorMapping = _studentMajors.FirstOrDefault(sm => sm.MajorId == majorId);
        if (majorMapping != null)
        {
            _studentMajors.Remove(majorMapping);
        }
    }

    /// <summary>
    /// Maps a specialized track focus onto this student's profile.
    /// </summary>
    /// <param name="specialtyId">The lookups reference key targeting the specialty sub-track.</param>
    public void AddSpecialty(Guid specialtyId)
    {
        if (specialtyId == Guid.Empty)
        {
            throw new ArgumentException("Specialty ID cannot be an empty Guid.", nameof(specialtyId));
        }

        if (_studentSpecialties.Any(ss => ss.SpecialtyId == specialtyId))
        {
            return; // Association already exists
        }

        _studentSpecialties.Add(new StudentSpecialty(Id, specialtyId));
    }

    /// <summary>
    /// Unmaps an existing specialty sub-track classification from this student's profile.
    /// </summary>
    /// <param name="specialtyId">The lookups reference tracking key to disconnect.</param>
    public void RemoveSpecialty(Guid specialtyId)
    {
        var specialtyMapping = _studentSpecialties.FirstOrDefault(ss => ss.SpecialtyId == specialtyId);
        if (specialtyMapping != null)
        {
            _studentSpecialties.Remove(specialtyMapping);
        }
    }

    /// <summary>
    /// Maps a professional skill competency identifier onto this student's profile.
    /// </summary>
    /// <param name="skillId">The lookups reference key targeting the technical skill.</param>
    public void AddSkill(Guid skillId)
    {
        if (skillId == Guid.Empty)
        {
            throw new ArgumentException("Skill ID cannot be an empty Guid.", nameof(skillId));
        }

        if (_studentSkills.Any(ss => ss.SkillId == skillId))
        {
            return; // Association already exists
        }

        _studentSkills.Add(new StudentSkill(Id, skillId));
    }

    /// <summary>
    /// Unmaps an existing technical skill competency from this student's profile.
    /// </summary>
    /// <param name="skillId">The lookups reference tracking key to disconnect.</param>
    public void RemoveSkill(Guid skillId)
    {
        var skillMapping = _studentSkills.FirstOrDefault(ss => ss.SkillId == skillId);
        if (skillMapping != null)
        {
            _studentSkills.Remove(skillMapping);
        }
    }
}