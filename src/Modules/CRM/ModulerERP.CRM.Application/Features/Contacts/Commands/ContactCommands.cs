using MediatR;
using ModulerERP.CRM.Application.DTOs;
using ModulerERP.CRM.Application.Interfaces;
using ModulerERP.CRM.Domain.Entities;
using ModulerERP.SharedKernel.Interfaces;
using ModulerERP.SharedKernel.ValueObjects;

namespace ModulerERP.CRM.Application.Features.Contacts.Commands;

// ── Create ──
public record CreateContactCommand(
    Guid PartnerId, string FirstName, string LastName,
    string? Position = null, string? Email = null, string? Phone = null,
    bool IsPrimary = false, AddressDto? Address = null) : IRequest<ContactDetailDto>;

public class CreateContactCommandHandler : IRequestHandler<CreateContactCommand, ContactDetailDto>
{
    private readonly IRepository<Contact> _repo;
    private readonly IRepository<BusinessPartner> _partnerRepo;
    private readonly ICRMUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CreateContactCommandHandler(IRepository<Contact> repo, IRepository<BusinessPartner> partnerRepo, ICRMUnitOfWork uow, ICurrentUserService currentUser)
    { _repo = repo; _partnerRepo = partnerRepo; _uow = uow; _currentUser = currentUser; }

    public async Task<ContactDetailDto> Handle(CreateContactCommand r, CancellationToken ct)
    {
        var partner = await _partnerRepo.GetByIdAsync(r.PartnerId, ct)
            ?? throw new ArgumentException("Partner not found.");

        var contact = Contact.Create(
            _currentUser.TenantId, r.PartnerId, r.FirstName, r.LastName,
            _currentUser.UserId, r.Position, r.Email, r.Phone, r.IsPrimary);

        if (r.Address != null)
            contact.UpdateAddress(new Address(r.Address.Street ?? "", r.Address.District ?? "", r.Address.City ?? "", r.Address.ZipCode ?? "", r.Address.Country ?? "", r.Address.Block ?? "", r.Address.Parcel ?? ""));

        await _repo.AddAsync(contact, ct);
        await _uow.SaveChangesAsync(ct);

        return new ContactDetailDto(contact.Id, contact.PartnerId, partner.Name,
            contact.FirstName, contact.LastName, contact.Position, contact.Email, contact.Phone, contact.IsPrimary,
            r.Address, contact.IsActive, contact.CreatedAt);
    }
}

// ── Update ──
public record UpdateContactCommand(
    Guid Id, string FirstName, string LastName,
    string? Position = null, string? Email = null, string? Phone = null,
    bool? IsPrimary = null, AddressDto? Address = null, bool? IsActive = null) : IRequest<ContactDetailDto>;

public class UpdateContactCommandHandler : IRequestHandler<UpdateContactCommand, ContactDetailDto>
{
    private readonly IRepository<Contact> _repo;
    private readonly ICRMUnitOfWork _uow;

    public UpdateContactCommandHandler(IRepository<Contact> repo, ICRMUnitOfWork uow) { _repo = repo; _uow = uow; }

    public async Task<ContactDetailDto> Handle(UpdateContactCommand r, CancellationToken ct)
    {
        var contact = await _repo.GetByIdAsync(r.Id, ct)
            ?? throw new KeyNotFoundException($"Contact '{r.Id}' not found.");

        contact.Update(r.FirstName, r.LastName, r.Position, r.Email, r.Phone);
        if (r.Address != null)
            contact.UpdateAddress(new Address(r.Address.Street ?? "", r.Address.District ?? "", r.Address.City ?? "", r.Address.ZipCode ?? "", r.Address.Country ?? "", r.Address.Block ?? "", r.Address.Parcel ?? ""));
        if (r.IsPrimary.HasValue) { if (r.IsPrimary.Value) contact.SetAsPrimary(); else contact.RemovePrimary(); }
        if (r.IsActive.HasValue) { if (r.IsActive.Value) contact.Activate(); else contact.Deactivate(); }

        await _uow.SaveChangesAsync(ct);
        return new ContactDetailDto(contact.Id, contact.PartnerId, "",
            contact.FirstName, contact.LastName, contact.Position, contact.Email, contact.Phone, contact.IsPrimary,
            null, contact.IsActive, contact.CreatedAt);
    }
}

// ── Delete ──
public record DeleteContactCommand(Guid Id) : IRequest;

public class DeleteContactCommandHandler : IRequestHandler<DeleteContactCommand>
{
    private readonly IRepository<Contact> _repo;
    private readonly ICRMUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public DeleteContactCommandHandler(IRepository<Contact> repo, ICRMUnitOfWork uow, ICurrentUserService currentUser)
    { _repo = repo; _uow = uow; _currentUser = currentUser; }

    public async Task Handle(DeleteContactCommand request, CancellationToken ct)
    {
        var contact = await _repo.GetByIdAsync(request.Id, ct)
            ?? throw new KeyNotFoundException($"Contact '{request.Id}' not found.");
        contact.Delete(_currentUser.UserId);
        await _uow.SaveChangesAsync(ct);
    }
}
