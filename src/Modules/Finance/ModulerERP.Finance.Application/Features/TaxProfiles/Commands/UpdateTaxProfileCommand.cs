using MediatR;
using ModulerERP.Finance.Application.DTOs;
using ModulerERP.Finance.Domain.Entities;
using ModulerERP.Finance.Domain.Enums;
using ModulerERP.SharedKernel.Interfaces;
using ModulerERP.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ModulerERP.Finance.Application.Features.TaxProfiles.Commands;

// UI doesn't provide CreatedByUserId in Update easily, so we just pass Id and DTO.
public record UpdateTaxProfileCommand(Guid Id, UpdateTaxProfileDto Dto, Guid UserId) : IRequest<Result<TaxProfileDto>>;

public class UpdateTaxProfileCommandHandler : IRequestHandler<UpdateTaxProfileCommand, Result<TaxProfileDto>>
{
    private readonly IRepository<TaxProfile> _profileRepo;
    private readonly IRepository<TaxRate> _rateRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public UpdateTaxProfileCommandHandler(
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

    public async Task<Result<TaxProfileDto>> Handle(UpdateTaxProfileCommand request, CancellationToken cancellationToken)
    {
        var profile = await _profileRepo.GetAllAsync(cancellationToken);
        var targetProfile = profile.FirstOrDefault(p => p.Id == request.Id);

        if (targetProfile == null)
            return Result<TaxProfileDto>.Failure("Tax Profile not found.");

        var dto = request.Dto;

        // The simplest approach to accommodate the generic repository missing robust Includes
        // is to clear existing rates logically mapped to this profile.
        // For the sake of this UI parity exercise, we'll recreate the tax rates if they changed, 
        // or just recreate the lines entirely to ensure the new rates/accounts are captured.
        // A full DDD implementation would carefully orchestrate the TaxRate entity updates.
        
        targetProfile.Lines.Clear();

        var tenantId = _currentUserService.TenantId;

        if (dto.VatRate > 0)
        {
            var vatRate = TaxRate.Create(tenantId, $"KDV-{dto.VatRate}", "KDV", TaxType.KDV, dto.VatRate, request.UserId, dto.VatAccountId);
            await _rateRepo.AddAsync(vatRate, cancellationToken);
            targetProfile.AddLine(vatRate.Id, true, 1);
        }

        if (dto.WithholdingRate > 0)
        {
            var stpRate = TaxRate.Create(tenantId, $"STP-{dto.WithholdingRate}", "Stopaj", TaxType.Stopaj, dto.WithholdingRate, request.UserId, dto.WithholdingAccountId);
            await _rateRepo.AddAsync(stpRate, cancellationToken);
            targetProfile.AddLine(stpRate.Id, false, 2);
        }

        if (dto.StampDutyRate > 0)
        {
            var stampRate = TaxRate.Create(tenantId, $"DMG-{dto.StampDutyRate}", "Damga", TaxType.Damga, dto.StampDutyRate, request.UserId, dto.StampDutyAccountId);
            await _rateRepo.AddAsync(stampRate, cancellationToken);
            targetProfile.AddLine(stampRate.Id, false, 3);
        }

        _profileRepo.Update(targetProfile);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var result = new TaxProfileDto
        {
            Id = targetProfile.Id,
            Name = dto.Name,
            Description = dto.Description,
            VatRate = dto.VatRate,
            VatAccountId = dto.VatAccountId,
            WithholdingRate = dto.WithholdingRate,
            WithholdingAccountId = dto.WithholdingAccountId,
            StampDutyRate = dto.StampDutyRate,
            StampDutyAccountId = dto.StampDutyAccountId,
            IsActive = dto.IsActive
        };

        return Result<TaxProfileDto>.Success(result);
    }
}
