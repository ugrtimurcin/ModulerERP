using MediatR;
using ModulerERP.CRM.Application.DTOs;
using ModulerERP.CRM.Application.Interfaces;
using ModulerERP.CRM.Domain.Entities;
using ModulerERP.CRM.Domain.Enums;
using ModulerERP.SharedKernel.Interfaces;
using ModulerERP.SharedKernel.ValueObjects;
using FluentValidation;

namespace ModulerERP.CRM.Application.Features.Partners.Commands;

// ── Create ──
public record CreatePartnerCommand(
    string Code, string Name, bool IsCustomer, bool IsSupplier, string Kind,
    Guid? GroupId = null, Guid? TerritoryId = null, Guid? DefaultCurrencyId = null,
    string? TaxOffice = null, string? TaxNumber = null, string? IdentityNumber = null,
    string? Email = null, string? MobilePhone = null,
    AddressDto? BillingAddress = null, AddressDto? ShippingAddress = null) : IRequest<BusinessPartnerDetailDto>;

public class CreatePartnerCommandValidator : AbstractValidator<CreatePartnerCommand>
{
    public CreatePartnerCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Kind).NotEmpty();
        RuleFor(x => x).Must(x => x.IsCustomer || x.IsSupplier)
            .WithMessage("Partner must be either customer or supplier.");
    }
}

public class CreatePartnerCommandHandler : IRequestHandler<CreatePartnerCommand, BusinessPartnerDetailDto>
{
    private readonly IRepository<BusinessPartner> _repo;
    private readonly ICRMUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CreatePartnerCommandHandler(IRepository<BusinessPartner> repo, ICRMUnitOfWork uow, ICurrentUserService currentUser)
    { _repo = repo; _uow = uow; _currentUser = currentUser; }

    public async Task<BusinessPartnerDetailDto> Handle(CreatePartnerCommand r, CancellationToken ct)
    {
        var kind = Enum.Parse<PartnerKind>(r.Kind);
        var partner = BusinessPartner.Create(
            _currentUser.TenantId, r.Code, r.Name, kind, r.IsCustomer, r.IsSupplier,
            _currentUser.UserId, r.GroupId, r.DefaultCurrencyId);

        if (!string.IsNullOrEmpty(r.TaxOffice) || !string.IsNullOrEmpty(r.TaxNumber) || !string.IsNullOrEmpty(r.IdentityNumber))
            partner.UpdateTaxInfo(r.TaxOffice, r.TaxNumber, r.IdentityNumber);
        if (!string.IsNullOrEmpty(r.Email) || !string.IsNullOrEmpty(r.MobilePhone))
            partner.UpdateContactInfo(null, r.Email, r.MobilePhone, null, null, null);
        if (r.BillingAddress != null)
            partner.UpdateBillingAddress(new Address(r.BillingAddress.Street ?? "", r.BillingAddress.District ?? "", r.BillingAddress.City ?? "", r.BillingAddress.ZipCode ?? "", r.BillingAddress.Country ?? "", r.BillingAddress.Block ?? "", r.BillingAddress.Parcel ?? ""));
        if (r.ShippingAddress != null)
            partner.UpdateShippingAddress(new Address(r.ShippingAddress.Street ?? "", r.ShippingAddress.District ?? "", r.ShippingAddress.City ?? "", r.ShippingAddress.ZipCode ?? "", r.ShippingAddress.Country ?? "", r.ShippingAddress.Block ?? "", r.ShippingAddress.Parcel ?? ""));
        if (r.TerritoryId.HasValue) 
            partner.SetTerritory(r.TerritoryId);

        await _repo.AddAsync(partner, ct);
        await _uow.SaveChangesAsync(ct);

        return MapToDetail(partner);
    }
    internal static BusinessPartnerDetailDto MapToDetail(BusinessPartner p) =>
        new(p.Id, p.Code, p.Name, p.IsCustomer, p.IsSupplier, p.Kind.ToString(),
            p.TaxOffice, p.TaxNumber, p.IdentityNumber, p.GroupId, p.TerritoryId, p.DefaultCurrencyId,
            p.DefaultDiscountRate,
            p.Website, p.Email, p.MobilePhone, p.Landline, p.Fax, p.WhatsappNumber,
            p.BillingAddress != null ? new AddressDto(p.BillingAddress.Street, p.BillingAddress.District, p.BillingAddress.City, p.BillingAddress.ZipCode, p.BillingAddress.Country, p.BillingAddress.Block, p.BillingAddress.Parcel) : null,
            p.ShippingAddress != null ? new AddressDto(p.ShippingAddress.Street, p.ShippingAddress.District, p.ShippingAddress.City, p.ShippingAddress.ZipCode, p.ShippingAddress.Country, p.ShippingAddress.Block, p.ShippingAddress.Parcel) : null,
            p.IsActive, p.CreatedAt);
}

// ── Update ──
public record UpdatePartnerCommand(
    Guid Id, string Name, bool IsCustomer, bool IsSupplier, string Kind,
    Guid? GroupId = null, Guid? TerritoryId = null, Guid? DefaultCurrencyId = null,
    string? TaxOffice = null, string? TaxNumber = null, string? IdentityNumber = null,
    string? Email = null, string? MobilePhone = null, string? Landline = null,
    string? Fax = null, string? WhatsappNumber = null, string? Website = null,
    decimal DefaultDiscountRate = 0,
    AddressDto? BillingAddress = null, AddressDto? ShippingAddress = null,
    bool? IsActive = null) : IRequest<BusinessPartnerDetailDto>;

public class UpdatePartnerCommandHandler : IRequestHandler<UpdatePartnerCommand, BusinessPartnerDetailDto>
{
    private readonly IRepository<BusinessPartner> _repo;
    private readonly ICRMUnitOfWork _uow;

    public UpdatePartnerCommandHandler(IRepository<BusinessPartner> repo, ICRMUnitOfWork uow)
    { _repo = repo; _uow = uow; }

    public async Task<BusinessPartnerDetailDto> Handle(UpdatePartnerCommand r, CancellationToken ct)
    {
        var partner = await _repo.GetByIdAsync(r.Id, ct)
            ?? throw new KeyNotFoundException($"Partner '{r.Id}' not found.");

        var kind = Enum.Parse<PartnerKind>(r.Kind);
        partner.UpdateBasicInfo(r.Name, kind, r.IsCustomer, r.IsSupplier);
        partner.UpdateTaxInfo(r.TaxOffice, r.TaxNumber, r.IdentityNumber);
        partner.UpdateFinancialInfo(r.DefaultCurrencyId, r.DefaultDiscountRate);
        partner.UpdateContactInfo(r.Website, r.Email, r.MobilePhone, r.Landline, r.Fax, r.WhatsappNumber);

        if (r.BillingAddress != null)
            partner.UpdateBillingAddress(new Address(r.BillingAddress.Street ?? "", r.BillingAddress.District ?? "", r.BillingAddress.City ?? "", r.BillingAddress.ZipCode ?? "", r.BillingAddress.Country ?? "", r.BillingAddress.Block ?? "", r.BillingAddress.Parcel ?? ""));
        if (r.ShippingAddress != null)
            partner.UpdateShippingAddress(new Address(r.ShippingAddress.Street ?? "", r.ShippingAddress.District ?? "", r.ShippingAddress.City ?? "", r.ShippingAddress.ZipCode ?? "", r.ShippingAddress.Country ?? "", r.ShippingAddress.Block ?? "", r.ShippingAddress.Parcel ?? ""));

        if (r.GroupId.HasValue) partner.SetGroup(r.GroupId);
        if (r.TerritoryId.HasValue) partner.SetTerritory(r.TerritoryId);
        if (r.IsActive.HasValue) { if (r.IsActive.Value) partner.Activate(); else partner.Deactivate(); }

        await _uow.SaveChangesAsync(ct);
        return CreatePartnerCommandHandler.MapToDetail(partner);
    }
}

// ── Delete ──
public record DeletePartnerCommand(Guid Id) : IRequest;

public class DeletePartnerCommandHandler : IRequestHandler<DeletePartnerCommand>
{
    private readonly IRepository<BusinessPartner> _repo;
    private readonly ICRMUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public DeletePartnerCommandHandler(IRepository<BusinessPartner> repo, ICRMUnitOfWork uow, ICurrentUserService currentUser)
    { _repo = repo; _uow = uow; _currentUser = currentUser; }

    public async Task Handle(DeletePartnerCommand request, CancellationToken ct)
    {
        var partner = await _repo.GetByIdAsync(request.Id, ct)
            ?? throw new KeyNotFoundException($"Partner '{request.Id}' not found.");
        partner.Delete(_currentUser.UserId);
        await _uow.SaveChangesAsync(ct);
    }
}
