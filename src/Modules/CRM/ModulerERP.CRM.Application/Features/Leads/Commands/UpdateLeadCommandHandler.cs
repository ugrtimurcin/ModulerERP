using MediatR;
using ModulerERP.CRM.Application.DTOs;
using ModulerERP.CRM.Application.Interfaces;
using ModulerERP.CRM.Domain.Entities;
using ModulerERP.CRM.Domain.Enums;
using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.CRM.Application.Features.Leads.Commands;

public class UpdateLeadCommandHandler : IRequestHandler<UpdateLeadCommand, LeadDetailDto>
{
    private readonly IRepository<Lead> _leadRepository;
    private readonly ICRMUnitOfWork _unitOfWork;

    public UpdateLeadCommandHandler(IRepository<Lead> leadRepository, ICRMUnitOfWork unitOfWork)
    {
        _leadRepository = leadRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<LeadDetailDto> Handle(UpdateLeadCommand request, CancellationToken cancellationToken)
    {
        var lead = await _leadRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Lead with Id '{request.Id}' not found.");

        if (!string.IsNullOrEmpty(request.Status) && Enum.TryParse<LeadStatus>(request.Status, out var status))
            lead.UpdateStatus(status);

        if (request.AssignedUserId.HasValue)
            lead.Assign(request.AssignedUserId.Value);

        if (request.IsActive.HasValue)
        {
            if (request.IsActive.Value) lead.Activate();
            else lead.Deactivate();
        }

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
