using AcademicGateway.Application.Common.Interfaces;
using AcademicGateway.Domain.Entities;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGateway.Application.Features.Providers.Commands.SubmitApplication;

public class SubmitProviderApplicationCommandHandler(IApplicationDbContext context)
    : IRequestHandler<SubmitProviderApplicationCommand, Guid>
{
    public async Task<Guid> Handle(SubmitProviderApplicationCommand request, CancellationToken cancellationToken)
    {
        // 1. Instantiate the Domain Profile Entity via its constructor (Defaults status to Draft)
        var providerApplication = new ProviderApplication(
            request.ProviderId,
            request.CompanyDetails,
            request.VerificationDocumentsUrl);

        // 2. Trigger the Domain State Transition Guard to move the state safely from Draft -> PendingReview
        providerApplication.SubmitForReview();

        // 3. Persist the workflow change to the database
        context.ProviderApplications.Add(providerApplication);
        await context.SaveChangesAsync(cancellationToken);

        // 4. Return the unique identifier for tracking purposes
        return providerApplication.Id;
    }
}