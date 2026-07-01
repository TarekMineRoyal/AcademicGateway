using System;
using Domain.Common.Exceptions;

namespace Domain.Professors.Exceptions;

/// <summary>
/// Exception thrown when a project allocation request is dispatched to a <see cref="Professor"/> 
/// whose active engagement tally has already reached or surpassed their maximum allowable boundary limit.
/// </summary>
public class ProfessorCapacityReachedException : DomainException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProfessorCapacityReachedException"/> class.
    /// </summary>
    /// <param name="professorId">The unique tracking identifier of the maxed-out professor aggregate.</param>
    public ProfessorCapacityReachedException(Guid professorId)
        : base($"Professor '{professorId}' has reached their maximum active supervision capacity constraints.", "PROFESSOR_CAPACITY_REACHED")
    {
    }
}