using System;
using System.Collections.Generic;
using System.Linq;
using AcademicGateway.Domain.Common;
using AcademicGateway.Domain.Professors.Exceptions;

namespace AcademicGateway.Domain.Professors;

/// <summary>
/// Represents a Professor aggregate root within the academic gateway profile subsystem.
/// Governs structural bio profiles, research indexing alignments, and active student project supervision capacity bounds.
/// </summary>
public class Professor : BaseEntity
{
    private readonly List<ProfessorResearchInterest> _researchInterests = new();

    /// <summary>
    /// Gets the unique user authentication tracking identity key matching this Professor profile.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the legal full name string matching this institutional faculty member.
    /// </summary>
    public string FullName { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the target academic department division designation text (e.g., "Computer Science").
    /// </summary>
    public string Department { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the professional rank title status holding for this faculty member (e.g., "Associate Professor").
    /// </summary>
    public string Rank { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the maximum ceiling boundary number of student projects this professor can supervise simultaneously.
    /// </summary>
    public int MaxSupervisionCapacity { get; private set; }

    /// <summary>
    /// Gets the current number of active project selections actively managed under this professor's guidance.
    /// </summary>
    public int CurrentProjectCount { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this professor possesses open slots to accept new project supervisions.
    /// </summary>
    public bool IsAcceptingProjects => CurrentProjectCount < MaxSupervisionCapacity;

    /// <summary>
    /// Gets a read-only encapsulated list of research mapping links initialized under this Professor profile.
    /// </summary>
    public IReadOnlyCollection<ProfessorResearchInterest> ResearchInterests => _researchInterests.AsReadOnly();

    /// <summary>
    /// Required parameterless constructor variant for Entity Framework Core relational database hydration mappings.
    /// </summary>
    private Professor()
    {
    }

    /// <summary>
    /// Initializes a new valid domain instance of the <see cref="Professor"/> aggregate root.
    /// </summary>
    /// <param name="id">The identification surrogate key mapped across identity configurations.</param>
    /// <param name="fullName">The target textual legal name of the professor.</param>
    /// <param name="department">The host academic home department string value mapping.</param>
    /// <param name="rank">The corporate faculty rank tier designation assignment text.</param>
    /// <param name="maxSupervisionCapacity">The raw ceiling project allocation capability allowed initially.</param>
    /// <exception cref="InvalidProfessorDetailsException">Thrown when a string attribute initialization component breaks rule limits.</exception>
    /// <exception cref="InvalidSupervisionCapacityException">Thrown when capacity properties map down to zero or less.</exception>
    public Professor(Guid id, string fullName, string department, string rank, int maxSupervisionCapacity)
    {
        if (id == Guid.Empty)
        {
            throw new InvalidProfessorDetailsException("Professor identity tracking reference context cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new InvalidProfessorDetailsException("Professor faculty identity full name cannot be empty or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(department))
        {
            throw new InvalidProfessorDetailsException("Academic department assignment details cannot be empty or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(rank))
        {
            throw new InvalidProfessorDetailsException("Faculty positional rank status details cannot be empty or whitespace.");
        }

        if (maxSupervisionCapacity <= 0)
        {
            throw new InvalidSupervisionCapacityException("Initial maximum supervisor project capacity limit bounds must exceed zero.");
        }

        Id = id;
        FullName = fullName.Trim();
        Department = department.Trim();
        Rank = rank.Trim();
        MaxSupervisionCapacity = maxSupervisionCapacity;
        CurrentProjectCount = 0;
    }

    /// <summary>
    /// Updates the core structural profile details, legal naming alignments, and professional career paths for this professor.
    /// </summary>
    /// <param name="fullName">The updated textual legal full name of the professor member.</param>
    /// <param name="department">The adjusted institutional department division tracking text assignment.</param>
    /// <param name="rank">The updated professional instructional positional rank status tier designation.</param>
    /// <exception cref="InvalidProfessorDetailsException">Thrown when any provided descriptive parameter string maps to empty or whitespace.</exception>
    public void UpdateFacultyDetails(string fullName, string department, string rank)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new InvalidProfessorDetailsException("Professor faculty identity full name cannot be empty or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(department))
        {
            throw new InvalidProfessorDetailsException("Academic department assignment details cannot be empty or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(rank))
        {
            throw new InvalidProfessorDetailsException("Faculty positional rank status details cannot be empty or whitespace.");
        }

        FullName = fullName.Trim();
        Department = department.Trim();
        Rank = rank.Trim();
    }

    /// <summary>
    /// Mutates the supervision boundary limitation rule values allocated to this professor profile asset.
    /// </summary>
    /// <param name="newCapacity">The targeted maximum count selection to assign onto the tracking records.</param>
    /// <exception cref="InvalidSupervisionCapacityException">Thrown when capacity maps below zero or drops under current active loads.</exception>
    public void UpdateSupervisionCapacity(int newCapacity)
    {
        if (newCapacity <= 0)
        {
            throw new InvalidSupervisionCapacityException("Altered maximum supervisor project capacity limit bounds must exceed zero.");
        }

        if (newCapacity < CurrentProjectCount)
        {
            throw new InvalidSupervisionCapacityException($"New capacity value configuration '{newCapacity}' cannot drop beneath the total of current active allocations ({CurrentProjectCount}).");
        }

        MaxSupervisionCapacity = newCapacity;
    }

    /// <summary>
    /// Increments the tracking payload index marking active student projects supervised by this professor.
    /// </summary>
    /// <exception cref="ProfessorCapacityReachedException">Thrown if availability states evaluate to false when this method fires.</exception>
    public void IncrementActiveProjects()
    {
        if (!IsAcceptingProjects)
        {
            throw new ProfessorCapacityReachedException(Id);
        }

        CurrentProjectCount++;
    }

    /// <summary>
    /// Decrements the active supervision project payload calculation upon clean completion or dropping context scenarios.
    /// </summary>
    public void DecrementActiveProjects()
    {
        if (CurrentProjectCount > 0)
        {
            CurrentProjectCount--;
        }
    }

    /// <summary>
    /// Attaches an underlying research interest connection index link configuration within this profile framework boundary.
    /// </summary>
    /// <param name="researchInterestId">The unique identifier tracking the targeted research interest asset definition.</param>
    /// <exception cref="InvalidProfessorDetailsException">Thrown when targeting a default empty Guid tracking parameter.</exception>
    public void AddResearchInterest(Guid researchInterestId)
    {
        if (researchInterestId == Guid.Empty)
        {
            throw new InvalidProfessorDetailsException("Target reference identity context for research alignment links cannot be empty.");
        }

        if (_researchInterests.Any(ri => ri.ResearchInterestId == researchInterestId))
        {
            return;
        }

        _researchInterests.Add(new ProfessorResearchInterest(Id, researchInterestId));
    }

    /// <summary>
    /// Clears an established research interest index linkage correlation model configuration from this profile aggregate.
    /// </summary>
    /// <param name="researchInterestId">The identification key of the research tracking profile target to break mapping rules from.</param>
    public void RemoveResearchInterest(Guid researchInterestId)
    {
        var interest = _researchInterests.FirstOrDefault(ri => ri.ResearchInterestId == researchInterestId);
        if (interest != null)
        {
            _researchInterests.Remove(interest);
        }
    }
}