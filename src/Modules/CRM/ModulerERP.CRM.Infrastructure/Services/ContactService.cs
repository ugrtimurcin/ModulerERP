using Microsoft.EntityFrameworkCore;
using ModulerERP.CRM.Application.DTOs;
using ModulerERP.CRM.Application.Interfaces;
using ModulerERP.CRM.Domain.Entities;
using ModulerERP.CRM.Infrastructure.Persistence;
using ModulerERP.SharedKernel.DTOs;

namespace ModulerERP.CRM.Infrastructure.Services;

public class ContactService : IContactService
{
    private readonly CRMDbContext _context;

    public ContactService(CRMDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<ContactListDto>> GetContactsAsync(Guid tenantId, int page, int pageSize, Guid? partnerId = null)
    {
        var query = _context.Contacts
            .Include(c => c.Partner)
            .Where(c => c.TenantId == tenantId);

        if (partnerId.HasValue)
            query = query.Where(c => c.PartnerId == partnerId);

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var data = await query
            .OrderBy(c => c.LastName).ThenBy(c => c.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new ContactListDto(
                c.Id,
                c.PartnerId,
                c.FirstName,
                c.LastName,
                c.FullName,
                c.Position,
                c.Email,
                c.Phone,
                c.IsPrimary,
                c.IsActive,
                c.CreatedAt))
            .ToListAsync();

        return new PagedResult<ContactListDto>(data, page, pageSize, totalCount, totalPages);
    }

    public async Task<ContactDetailDto?> GetContactByIdAsync(Guid tenantId, Guid id)
    {
        var contact = await _context.Contacts
            .Include(c => c.Partner)
            .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Id == id);

        if (contact == null) return null;

        return new ContactDetailDto(
            contact.Id,
            contact.PartnerId,
            contact.Partner!.Name,
            contact.FirstName,
            contact.LastName,
            contact.Position,
            contact.Email,
            contact.Phone,
            contact.IsPrimary,
            contact.IsActive,
            contact.CreatedAt);
    }

    public async Task<ContactDetailDto> CreateContactAsync(Guid tenantId, CreateContactDto dto, Guid createdByUserId)
    {
        var partnerExists = await _context.BusinessPartners
            .AnyAsync(p => p.TenantId == tenantId && p.Id == dto.PartnerId);
        
        if (!partnerExists)
            throw new ArgumentException("Partner not found");

        var contact = Contact.Create(
            tenantId,
            dto.PartnerId,
            dto.FirstName,
            dto.LastName,
            createdByUserId,
            dto.Position,
            dto.Email,
            dto.Phone,
            dto.IsPrimary);

        if (dto.IsPrimary)
        {
            // Reset other primary contacts for this partner
            var existingPrimary = await _context.Contacts
                .Where(c => c.TenantId == tenantId && c.PartnerId == dto.PartnerId && c.IsPrimary)
                .ToListAsync();
            
            foreach (var c in existingPrimary)
                c.RemovePrimary();
        }

        _context.Contacts.Add(contact);
        await _context.SaveChangesAsync();

        return (await GetContactByIdAsync(tenantId, contact.Id))!;
    }

    public async Task<ContactDetailDto> UpdateContactAsync(Guid tenantId, Guid id, UpdateContactDto dto)
    {
        var contact = await _context.Contacts
            .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Id == id)
            ?? throw new KeyNotFoundException("Contact not found");

        contact.Update(
            dto.FirstName, 
            dto.LastName, 
            dto.Position, 
            dto.Email, 
            dto.Phone);

        if (dto.IsPrimary.HasValue)
        {
            if (dto.IsPrimary.Value)
            {
                // Reset other primary contacts
                var existingPrimary = await _context.Contacts
                    .Where(c => c.TenantId == tenantId && c.PartnerId == contact.PartnerId && c.IsPrimary && c.Id != id)
                    .ToListAsync();
                
                foreach (var c in existingPrimary)
                    c.RemovePrimary();
                
                contact.SetAsPrimary();
            }
            else
            {
                contact.RemovePrimary();
            }
        }

        if (dto.IsActive.HasValue)
        {
            if (dto.IsActive.Value) contact.Activate();
            else contact.Deactivate();
        }

        await _context.SaveChangesAsync();
        return (await GetContactByIdAsync(tenantId, id))!;
    }

    public async Task DeleteContactAsync(Guid tenantId, Guid id, Guid deletedByUserId)
    {
        var contact = await _context.Contacts
            .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Id == id)
            ?? throw new KeyNotFoundException("Contact not found");

        contact.Delete(deletedByUserId);
        await _context.SaveChangesAsync();
    }

    public async Task SetPrimaryContactAsync(Guid tenantId, Guid partnerId, Guid contactId)
    {
        var contact = await _context.Contacts
            .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Id == contactId && c.PartnerId == partnerId)
            ?? throw new KeyNotFoundException("Contact not found");

        var existingPrimary = await _context.Contacts
            .Where(c => c.TenantId == tenantId && c.PartnerId == partnerId && c.IsPrimary)
            .ToListAsync();
        
        foreach (var c in existingPrimary)
            c.RemovePrimary();

        contact.SetAsPrimary();
        await _context.SaveChangesAsync();
    }
}
