namespace ModulerERP.SharedKernel.Interfaces;

public interface ISalesOperationsService
{
    Task<Guid> CreateInvoiceFromProjectPaymentAsync(Guid tenantId, Guid customerId, decimal amount, string description, Guid currencyId);
}
