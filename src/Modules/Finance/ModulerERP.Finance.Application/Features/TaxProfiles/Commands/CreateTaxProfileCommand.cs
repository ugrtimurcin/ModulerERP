using MediatR;
using ModulerERP.Finance.Application.DTOs;
using ModulerERP.Finance.Domain.Entities;
using ModulerERP.Finance.Domain.Enums;
using ModulerERP.SharedKernel.Interfaces;
using ModulerERP.SharedKernel.Results;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ModulerERP.Finance.Application.Features.TaxProfiles.Commands;

public record CreateTaxProfileCommand(CreateTaxProfileDto Dto, Guid CreatedByUserId) : IRequest<Result<TaxProfileDto>>;

public class CreateTaxProfileCommandHandler : IRequestHandler<CreateTaxProfileCommand, Result<TaxProfileDto>>
{
    private readonly IRepository<TaxProfile> _profileRepo;
    private readonly IRepository<TaxRate> _rateRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CreateTaxProfileCommandHandler(
        IRepository<TaxProfile> profileRepo,
        IRepository<TaxRate> rateRepo,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _profileRepo = profileRepo;
        _rateRepo = rateRepo;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<TaxProfileDto>> Handle(CreateTaxProfileCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        var tenantId = _currentUserService.TenantId;

        var profile = TaxProfile.Create(
            tenantId, 
            $"TP-{Guid.NewGuid().ToString()[..6]}", 
            dto.Name, 
            request.CreatedByUserId, 
            dto.Description);

        // KDV
        if (dto.VatRate > 0)
        {
            var vatRate = TaxRate.Create(tenantId, $"KDV-{dto.VatRate}", "KDV", TaxType.KDV, dto.VatRate, request.CreatedByUserId, dto.VatAccountId);
            await _rateRepo.AddAsync(vatRate, cancellationToken);
            profile.AddLine(vatRate.Id, true, 1);
        }

        // Stopaj
        if (dto.WithholdingRate > 0)
        {
            var stpRate = TaxRate.Create(tenantId, $"STP-{dto.WithholdingRate}", "Stopaj", TaxType.Stopaj, dto.WithholdingRate, request.CreatedByUserId, dto.WithholdingAccountId);
            await _rateRepo.AddAsync(stpRate, cancellationToken);
            profile.AddLine(stpRate.Id, false, 2); // Stopaj is conventionally exclusive
        }

        // Damga
        if (dto.StampDutyRate > 0)
        {
            var stampRate = TaxRate.Create(tenantId, $"DMG-{dto.StampDutyRate}", "Damga", TaxType.Damga, dto.StampDutyRate, request.CreatedByUserId, dto.StampDutyAccountId);
            await _rateRepo.AddAsync(stampRate, cancellationToken);
            profile.AddLine(stampRate.Id, false, 3);
        }

        await _profileRepo.AddAsync(profile, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var result = new TaxProfileDto
        {
            Id = profile.Id,
            Name = profile.Name,
            Description = profile.Description ?? string.Empty,
            VatRate = dto.VatRate,
            VatAccountId = dto.VatAccountId,
            WithholdingRate = dto.WithholdingRate,
            WithholdingAccountId = dto.WithholdingAccountId,
            StampDutyRate = dto.StampDutyRate,
            StampDutyAccountId = dto.StampDutyAccountId,
            IsActive = true
        };

        return Result<TaxProfileDto>.Success(result);
    }
}
