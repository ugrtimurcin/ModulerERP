using ModulerERP.SharedKernel.Results;

namespace ModulerERP.SharedKernel.Interfaces;

public interface IFinanceOperationsService
{
    Task<Result> CreateReceivableAsync(
        Guid tenantId,
        string invoiceNumber,
        Guid partnerId,
        decimal amount,
        string currencyCode,
        DateTime invoiceDate,
        DateTime dueDate,
        Guid? sourceDocumentId = null,
        string? description = null,
        CancellationToken cancellationToken = default);
}
