using MediatR;
using ModulerERP.CRM.Application.DTOs;
using ModulerERP.CRM.Application.Interfaces;
using ModulerERP.CRM.Domain.Entities;
using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.CRM.Application.Features.Leads.Commands;

public class CreateLeadCommandHandler : IRequestHandler<CreateLeadCommand, LeadDetailDto>
{
    private readonly IRepository<Lead> _leadRepository;
    private readonly ICRMUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CreateLeadCommandHandler(
        IRepository<Lead> leadRepository,
        ICRMUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _leadRepository = leadRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<LeadDetailDto> Handle(CreateLeadCommand request, CancellationToken cancellationToken)
    {
        var lead = Lead.Create(
            _currentUserService.TenantId,
            request.FirstName,
            request.LastName,
            _currentUserService.UserId,
            request.Title,
            request.Company,
            request.Email,
            request.Phone,
            request.Source,
            request.AssignedUserId);

        await _leadRepository.AddAsync(lead, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new LeadDetailDto(
            lead.Id,
            lead.Title,
            lead.FirstName,
            lead.LastName,
            lead.Company,
            lead.Email,
            lead.Phone,
            lead.Status.ToString(),
            lead.Source,
            lead.AssignedUserId,
            null,
            lead.ConvertedPartnerId,
            lead.ConvertedAt,
            lead.IsActive,
            lead.CreatedAt);
    }
}
