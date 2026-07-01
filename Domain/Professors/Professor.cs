using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Common;

namespace Domain.Professors;

/// <summary>
/// Represents an academic faculty profile capable of supervising student projects, 
/// evaluating submissions, and defining research parameters.
/// </summary>
public class Professor : BaseEntity
{
    private readonly List<ProfessorResearchInterest> _professorResearchInterests = new();

    /// <summary>
    /// Gets the unique identifier for the Professor profile. 
    /// Maps 1:1 to the underlying Identity ApplicationUser identifier.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the formal academic title or rank (e.g., Assistant Professor, Associate Professor).
    /// </summary>
    public string AcademicRank { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the primary generalized department alignment (e.g., Information Technology).
    /// </summary>
    public string Department { get; private set; } = string.Empty;

    /// <summary>
    /// Gets a biography describing the professor's academic journey and statements.
    /// </summary>
    public string AboutMe { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the physical or digital office coordinates for consultations.
    /// </summary>
    public string OfficeLocation { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the maximum number of student projects this professor can simultaneously supervise.
    /// </summary>
    public int MaxProjectCapacity { get; private set; }

    /// <summary>
    /// Gets the current number of active student projects being supervised.
    /// </summary>
    public int CurrentProjectCount { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this professor has undergone credentials verification 
    /// by institutional administrators to certify their academic title.
    /// </summary>
    public bool IsVerified { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this professor is actively accepting new student project supervisions.
    /// </summary>
    public bool IsAcceptingProjects => CurrentProjectCount < MaxProjectCapacity;

    /// <summary>
    /// Gets the read-only collaborative collection tracking granular research focuses assigned to this profile.
    /// </summary>
    public IReadOnlyCollection<ProfessorResearchInterest> ProfessorResearchInterests => _professorResearchInterests.AsReadOnly();

    /// <summary>
    /// EF Core constructor requirement. Prevents bypass of domain constraints during hydration.
    /// </summary>
    private Professor()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Professor"/> profile with default limits and unverified state.
    /// </summary>
    /// <param name="id">The unique Identity key linking back to the account credentials.</param>
    /// <param name="department">The generalized institutional department assignment.</param>
    /// <param name="academicRank">The formal academic rank title.</param>
    /// <exception cref="ArgumentException">Thrown when essential tracking values are missing.</exception>
    public Professor(Guid id, string department, string academicRank)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Identity User ID cannot be empty.", nameof(id));
        }

        Id = id;
        UpdateInstitutionalDetails(department, academicRank);

        MaxProjectCapacity = 3;
        CurrentProjectCount = 0;
        IsVerified = false; // Initial status is unverified until background certification executes
    }

    /// <summary>
    /// Updates the core institutional fields for the faculty member.
    /// </summary>
    public void UpdateInstitutionalDetails(string department, string academicRank)
    {
        if (string.IsNullOrWhiteSpace(department))
        {
            throw new ArgumentException("Department assignment cannot be empty.", nameof(department));
        }

        if (string.IsNullOrWhiteSpace(academicRank))
        {
            throw new ArgumentException("Academic rank/title cannot be empty.", nameof(academicRank));
        }

        Department = department.Trim();
        AcademicRank = academicRank.Trim();
    }

    /// <summary>
    /// Updates the personalized narrative summaries utilized by students to locate matching advisors.
    /// </summary>
    /// <param name="aboutMe">The personal statement bio summary.</param>
    /// <param name="officeLocation">The contact location placement text.</param>
    public void UpdateBio(string aboutMe, string officeLocation)
    {
        AboutMe = aboutMe?.Trim() ?? string.Empty;
        OfficeLocation = officeLocation?.Trim() ?? string.Empty;
    }

    /// <summary>
    /// Formally authenticates and certifies the credentials claim of the faculty account member.
    /// Reserved for internal administration workflow triggers.
    /// </summary>
    public void VerifyProfile()
    {
        IsVerified = true;
    }

    /// <summary>
    /// Revokes verified standing due to compliance updates or profile modifications.
    /// </summary>
    public void RevokeVerification()
    {
        IsVerified = false;
    }

    /// <summary>
    /// Maps a specialized research keyword track onto this professor's profile.
    /// </summary>
    /// <param name="researchInterestId">The lookups reference key targeting the research entry.</param>
    public void AddResearchInterest(Guid researchInterestId)
    {
        if (researchInterestId == Guid.Empty)
        {
            throw new ArgumentException("Research Interest ID cannot be an empty Guid.", nameof(researchInterestId));
        }

        if (_professorResearchInterests.Any(pri => pri.ResearchInterestId == researchInterestId))
        {
            return; // Already mapped
        }

        _professorResearchInterests.Add(new ProfessorResearchInterest(Id, researchInterestId));
    }

    /// <summary>
    /// Unmaps an existing research keyword classification from this professor's profile.
    /// </summary>
    /// <param name="researchInterestId">The lookups reference tracking key to disconnect.</param>
    public void RemoveResearchInterest(Guid researchInterestId)
    {
        var interestMapping = _professorResearchInterests.FirstOrDefault(pri => pri.ResearchInterestId == researchInterestId);
        if (interestMapping != null)
        {
            _professorResearchInterests.Remove(interestMapping);
        }
    }

    /// <summary>
    /// Updates the operational supervision workload limit thresholds.
    /// </summary>
    public void UpdateSupervisionCapacity(int newCapacity)
    {
        if (newCapacity < CurrentProjectCount)
        {
            throw new ArgumentException($"New capacity limit ({newCapacity}) cannot be less than the current active project count ({CurrentProjectCount}).");
        }

        MaxProjectCapacity = newCapacity;
    }

    /// <summary>
    /// Increment execution hook called natively when a project submission is finalized under this professor.
    /// </summary>
    public void IncrementActiveProjects()
    {
        if (!IsAcceptingProjects)
        {
            throw new InvalidOperationException("This professor has reached their maximum active supervision capacity.");
        }

        CurrentProjectCount++;
    }

    /// <summary>
    /// Decrement execution hook called natively when a project is completed, cancelled, or closed out.
    /// </summary>
    public void DecrementActiveProjects()
    {
        if (CurrentProjectCount > 0)
        {
            CurrentProjectCount--;
        }
    }
}