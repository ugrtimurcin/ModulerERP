using MediatR;
using ModulerERP.CRM.Application.Interfaces;
using ModulerERP.CRM.Domain.Entities;
using ModulerERP.CRM.Domain.Enums;
using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.CRM.Application.Features.Leads.Commands;

public record ConvertLeadToPartnerCommand(Guid LeadId) : IRequest<Guid>;

public class ConvertLeadToPartnerCommandHandler : IRequestHandler<ConvertLeadToPartnerCommand, Guid>
{
    private readonly IRepository<Lead> _leadRepository;
    private readonly IRepository<BusinessPartner> _partnerRepository;
    private readonly ICRMUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public ConvertLeadToPartnerCommandHandler(
        IRepository<Lead> leadRepository,
        IRepository<BusinessPartner> partnerRepository,
        ICRMUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _leadRepository = leadRepository;
        _partnerRepository = partnerRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Guid> Handle(ConvertLeadToPartnerCommand request, CancellationToken cancellationToken)
    {
        var lead = await _leadRepository.GetByIdAsync(request.LeadId, cancellationToken)
            ?? throw new KeyNotFoundException($"Lead with Id '{request.LeadId}' not found.");

        if (lead.Status == LeadStatus.Qualified || lead.ConvertedPartnerId.HasValue)
            throw new InvalidOperationException("Lead is already converted.");

        var partnerCode = "P-" + DateTime.UtcNow.Ticks.ToString()[10..];
        var partner = BusinessPartner.Create(
            _currentUserService.TenantId,
            partnerCode,
            lead.Company ?? lead.FullName,
            lead.Company != null ? PartnerKind.Company : PartnerKind.Individual,
            true,
            false,
            _currentUserService.UserId);

        if (!string.IsNullOrEmpty(lead.Email) || !string.IsNullOrEmpty(lead.Phone))
            partner.UpdateContactInfo(null, lead.Email, lead.Phone, null, null, null);

        await _partnerRepository.AddAsync(partner, cancellationToken);
        lead.ConvertToPartner(partner.Id);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return partner.Id;
    }
}
