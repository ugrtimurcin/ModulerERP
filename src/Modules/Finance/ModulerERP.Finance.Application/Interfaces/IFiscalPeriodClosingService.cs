using ModulerERP.SharedKernel.Results;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ModulerERP.Finance.Application.Interfaces;

/// <summary>
/// Service responsible for handling Month-End closing procedures,
/// including Foreign Exchange Gain/Loss Revaluation and Period State Management.
/// </summary>
public interface IFiscalPeriodClosingService
{
    Task<Result> ClosePeriodAsync(Guid tenantId, Guid periodId, CancellationToken cancellationToken = default);
}
