namespace ModulerERP.SharedKernel.DTOs;

public record CurrencyLookupDto(Guid Id, string Code, string Symbol, bool IsActive);
