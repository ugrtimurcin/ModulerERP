namespace ModulerERP.CRM.Application.DTOs;

// Tag DTOs
public record TagListDto(Guid Id, string Name, string ColorCode, string? EntityType);
public record TagDetailDto(Guid Id, string Name, string ColorCode, string? EntityType, DateTime CreatedAt);
public record CreateTagDto(string Name, string ColorCode = "#3B82F6", string? EntityType = null);
public record UpdateTagDto(string Name, string ColorCode, string? EntityType = null);

// EntityTag DTOs
public record EntityTagDto(Guid Id, Guid TagId, string TagName, string TagColor);
public record CreateEntityTagDto(Guid TagId, Guid EntityId, string EntityType);
