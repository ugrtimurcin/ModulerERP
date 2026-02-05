namespace ModulerERP.SharedKernel.DTOs;

/// <summary>
/// Generic paged result DTO for paginated API responses.
/// </summary>
public record PagedResult<T>(
    IEnumerable<T> Data, 
    int Page, 
    int PageSize, 
    int TotalCount, 
    int TotalPages);
